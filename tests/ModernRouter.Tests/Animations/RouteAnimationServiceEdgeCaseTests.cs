using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ModernRouter.Animations;
using Moq;
using System.Reflection;
using Xunit;

namespace ModernRouter.Tests.Animations;

public class RouteAnimationServiceEdgeCaseTests
{
    private readonly Mock<IJSRuntimeWrapper> _mockJSRuntime;
    private readonly IRouteAnimationService _animationService;

    public RouteAnimationServiceEdgeCaseTests()
    {
        _mockJSRuntime = new Mock<IJSRuntimeWrapper>();
        _animationService = new RouteAnimationService(_mockJSRuntime.Object);
    }

    [Fact]
    public void Constructor_RegistersBuiltInAnimations()
    {
        var animations = new[] { "slideLeft", "slideRight", "slideUp", "slideDown", "fadeIn", "fadeOut", "scaleIn", "scaleOut", "zoomIn", "zoomOut" };

        foreach (var animationName in animations)
        {
            var animation = _animationService.GetAnimation(animationName);
            animation.Should().NotBeNull($"built-in animation '{animationName}' should be registered");
        }
    }

    [Fact]
    public async Task BeginExitAsync_WithAnimationDisabled_ReturnsNone()
    {
        // Disable animations through reflection
        var field = _animationService.GetType().GetField("IsAnimationEnabled", BindingFlags.Public | BindingFlags.Instance);
        field?.SetValue(_animationService, false);

        var component = new TestComponent();
        var context = CreateAnimationContext();

        var result = await _animationService.BeginExitAsync(component, context);

        result.Should().BeEquivalentTo(AnimationResult.None);
    }

    [Fact]
    public async Task BeginExitAsync_WithCancelledToken_ReturnsNone()
    {
        var component = new TestComponent();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var context = CreateAnimationContext(cts.Token);

        var result = await _animationService.BeginExitAsync(component, context);

        result.Should().BeEquivalentTo(AnimationResult.None);
    }

    [Fact]
    public async Task BeginExitAsync_WithComponentWithoutAnimation_ReturnsNone()
    {
        var component = new TestComponent();
        var context = CreateAnimationContext();

        var result = await _animationService.BeginExitAsync(component, context);

        result.Should().BeEquivalentTo(AnimationResult.None);
    }

    [Fact]
    public async Task BeginExitAsync_WithJSRuntimeException_ReturnsFailedResult()
    {
        _mockJSRuntime.Setup(x => x.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("JS Error"));

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext();

        var result = await _animationService.BeginExitAsync(component, context);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Exit animation failed");
    }

    [Fact]
    public async Task BeginExitAsync_WithJSDisconnectedException_HandlesGracefully()
    {
        _mockJSRuntime.Setup(x => x.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
            .ThrowsAsync(new JSDisconnectedException("Circuit disconnected"));

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext();

        var result = await _animationService.BeginExitAsync(component, context);

        // Should handle JSDisconnectedException gracefully and return successful result
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BeginExitAsync_WithCancellationDuringAnimation_ReturnsFailedResult()
    {
        var cts = new CancellationTokenSource();
        
        _mockJSRuntime.Setup(x => x.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
            .Returns(async (string identifier, CancellationToken token, object[] args) =>
            {
                await Task.Delay(100, token); // Simulate async JS call
            });

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext(cts.Token);

        // Cancel the token after starting the animation
        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            cts.Cancel();
        });

        var result = await _animationService.BeginExitAsync(component, context);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Animation cancelled");
    }

    [Fact]
    public async Task BeginEnterAsync_WithAnimationDisabled_ReturnsNone()
    {
        // Disable animations through reflection
        var field = _animationService.GetType().GetField("IsAnimationEnabled", BindingFlags.Public | BindingFlags.Instance);
        field?.SetValue(_animationService, false);

        var component = new TestComponent();
        var context = CreateAnimationContext();

        var result = await _animationService.BeginEnterAsync(component, context);

        result.Should().BeEquivalentTo(AnimationResult.None);
    }

    [Fact]
    public async Task BeginEnterAsync_WithComponentWithoutAnimation_ReturnsNone()
    {
        var component = new TestComponent();
        var context = CreateAnimationContext();

        var result = await _animationService.BeginEnterAsync(component, context);

        result.Should().BeEquivalentTo(AnimationResult.None);
    }

    [Fact]
    public async Task BeginEnterAsync_WithJSRuntimeException_ReturnsFailedResult()
    {
        _mockJSRuntime.Setup(x => x.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("JS Error"));

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext();

        var result = await _animationService.BeginEnterAsync(component, context);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Enter animation failed");
    }

    [Fact]
    public async Task ConcurrentAnimations_WithSemaphore_ExecuteSequentially()
    {
        var executionOrder = new List<int>();
        var semaphoreCounter = 0;

        _mockJSRuntime.Setup(x => x.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
            .Returns<string, CancellationToken, object[]>(async (identifier, token, args) =>
            {
                var currentCount = Interlocked.Increment(ref semaphoreCounter);
                executionOrder.Add(currentCount);
                await Task.Delay(100, token);
                Interlocked.Decrement(ref semaphoreCounter);
            });

        var component1 = new TestAnimatedComponent();
        var component2 = new TestAnimatedComponent();
        var context1 = CreateAnimationContext();
        var context2 = CreateAnimationContext();

        var task1 = _animationService.BeginExitAsync(component1, context1);
        var task2 = _animationService.BeginExitAsync(component2, context2);

        await Task.WhenAll(task1, task2);

        // Verify animations executed sequentially (semaphore count never exceeded 1)
        executionOrder.Should().AllSatisfy(count => count.Should().BeLessOrEqualTo(1));
    }

    [Fact]
    public void RegisterAnimation_WithValidAnimation_RegistersSuccessfully()
    {
        var customAnimation = new RouteAnimation
        {
            EnterKeyframes = "opacity: 0 => opacity: 1",
            ExitKeyframes = "opacity: 1 => opacity: 0",
            Duration = TimeSpan.FromMilliseconds(300)
        };

        _animationService.RegisterAnimation("custom", customAnimation);

        var retrieved = _animationService.GetAnimation("custom");
        retrieved.Should().Be(customAnimation);
    }

    [Fact]
    public void RegisterAnimation_WithDuplicateName_OverwritesExisting()
    {
        var animation1 = new RouteAnimation { Duration = TimeSpan.FromMilliseconds(100) };
        var animation2 = new RouteAnimation { Duration = TimeSpan.FromMilliseconds(200) };

        _animationService.RegisterAnimation("test", animation1);
        _animationService.RegisterAnimation("test", animation2);

        var retrieved = _animationService.GetAnimation("test");
        retrieved.Should().Be(animation2);
        retrieved!.Duration.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Fact]
    public void GetAnimation_WithNonExistentName_ReturnsNull()
    {
        var result = _animationService.GetAnimation("nonexistent");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetAnimation_WithNullOrEmptyName_ReturnsNull(string? animationName)
    {
        var result = _animationService.GetAnimation(animationName!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task BeginExitAsync_WithAnimationOverride_UsesOverrideAnimation()
    {
        var customAnimation = new RouteAnimation
        {
            ExitKeyframes = "opacity: 1 => opacity: 0",
            Duration = TimeSpan.FromMilliseconds(100)
        };
        _animationService.RegisterAnimation("customExit", customAnimation);

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext(animationOverride: "customExit");

        var result = await _animationService.BeginExitAsync(component, context);

        result.Success.Should().BeTrue();
        result.Duration.Should().Be(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task BeginEnterAsync_WithAnimationOverride_UsesOverrideAnimation()
    {
        var customAnimation = new RouteAnimation
        {
            EnterKeyframes = "opacity: 0 => opacity: 1",
            Duration = TimeSpan.FromMilliseconds(100)
        };
        _animationService.RegisterAnimation("customEnter", customAnimation);

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext(animationOverride: "customEnter");

        var result = await _animationService.BeginEnterAsync(component, context);

        result.Success.Should().BeTrue();
        result.Duration.Should().Be(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task BeginExitAsync_WithRapidCancellation_HandlesCorrectly()
    {
        var component = new TestAnimatedComponent();
        var contexts = new List<RouteAnimationContext>();
        var tasks = new List<Task<AnimationResult>>();

        // Create multiple rapid animation requests with cancellation
        for (int i = 0; i < 10; i++)
        {
            var cts = new CancellationTokenSource();
            var context = CreateAnimationContext(cts.Token);
            contexts.Add(context);

            var task = _animationService.BeginExitAsync(component, context);
            tasks.Add(task);

            // Cancel some randomly
            if (i % 3 == 0)
            {
                cts.Cancel();
            }
        }

        var results = await Task.WhenAll(tasks);

        // Should handle all requests without throwing exceptions
        results.Should().AllSatisfy(result => result.Should().NotBeNull());
    }

    [Fact]
    public async Task BeginExitAsync_WithMalformedKeyframes_HandlesGracefully()
    {
        var malformedAnimation = new RouteAnimation
        {
            ExitKeyframes = "invalid keyframes format",
            Duration = TimeSpan.FromMilliseconds(100)
        };
        _animationService.RegisterAnimation("malformed", malformedAnimation);

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext(animationOverride: "malformed");

        var result = await _animationService.BeginExitAsync(component, context);

        // Should handle malformed keyframes gracefully
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BeginEnterAsync_WithExtremelyLongDuration_HandlesCorrectly()
    {
        var longAnimation = new RouteAnimation
        {
            EnterKeyframes = "opacity: 0 => opacity: 1",
            Duration = TimeSpan.FromHours(1) // Extremely long duration
        };
        _animationService.RegisterAnimation("longDuration", longAnimation);

        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Cancel after 100ms
        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext(cts.Token, "longDuration");

        var result = await _animationService.BeginEnterAsync(component, context);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Animation cancelled");
    }

    [Fact]
    public async Task BeginExitAsync_WithZeroDuration_CompletesImmediately()
    {
        var zeroDurationAnimation = new RouteAnimation
        {
            ExitKeyframes = "opacity: 1 => opacity: 0",
            Duration = TimeSpan.Zero
        };
        _animationService.RegisterAnimation("zeroDuration", zeroDurationAnimation);

        var component = new TestAnimatedComponent();
        var context = CreateAnimationContext(animationOverride: "zeroDuration");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _animationService.BeginExitAsync(component, context);
        stopwatch.Stop();

        result.Success.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "zero duration animation should complete quickly");
    }

    private static RouteAnimationContext CreateAnimationContext(CancellationToken? cancellationToken = null, string? animationOverride = null)
    {
        return new RouteAnimationContext
        {
            CancellationToken = cancellationToken ?? CancellationToken.None,
            AnimationOverride = animationOverride
        };
    }

    // Test components
    private class TestComponent : ComponentBase { }

    [RouteAnimation("fadeIn", "fadeOut", Duration = 300)]
    private class TestAnimatedComponent : ComponentBase { }
}