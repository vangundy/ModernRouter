using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ModernRouter.Animations;

public sealed class RouteAnimationService : IRouteAnimationService
{
    private readonly IJSRuntimeWrapper _jsRuntime;
    private readonly Dictionary<string, RouteAnimation> _animations = new();
    private readonly SemaphoreSlim _animationSemaphore = new(1, 1);
    private static int _animationCounter = 0;

    public RouteAnimationService(IJSRuntimeWrapper jsRuntime)
    {
        _jsRuntime = jsRuntime;
        RegisterBuiltInAnimations();
    }

    public bool IsAnimationEnabled { get; private set; } = true;

    public async Task<AnimationResult> BeginExitAsync(ComponentBase component, RouteAnimationContext context)
    {
        if (!IsAnimationEnabled || context.CancellationToken.IsCancellationRequested)
            return AnimationResult.None;

        var animation = GetAnimationForComponent(component, context);
        if (animation == null || string.IsNullOrEmpty(animation.ExitKeyframes))
            return AnimationResult.None;

        var semaphoreAcquired = false;
        try
        {
            await _animationSemaphore.WaitAsync(context.CancellationToken);
            semaphoreAcquired = true;
            
            var animationId = $"route-exit-{Interlocked.Increment(ref _animationCounter)}";
            var css = GenerateAnimationCss(animationId, animation.ExitKeyframes, animation);
            
            await InjectAnimationCss(css, context.CancellationToken);
            await ApplyAnimationToElement(component, animationId, context.CancellationToken);
            
            // Wait for animation duration
            await Task.Delay(animation.Duration, context.CancellationToken);
            
            return AnimationResult.Successful(animation.Duration, css);
        }
        catch (OperationCanceledException)
        {
            return AnimationResult.Failed("Animation cancelled");
        }
        catch (Exception ex)
        {
            return AnimationResult.Failed($"Exit animation failed: {ex.Message}");
        }
        finally
        {
            if (semaphoreAcquired)
                _animationSemaphore.Release();
        }
    }

    public async Task<AnimationResult> BeginEnterAsync(ComponentBase component, RouteAnimationContext context)
    {
        if (!IsAnimationEnabled || context.CancellationToken.IsCancellationRequested)
            return AnimationResult.None;

        var animation = GetAnimationForComponent(component, context);
        if (animation == null || string.IsNullOrEmpty(animation.EnterKeyframes))
            return AnimationResult.None;

        var semaphoreAcquired = false;
        try
        {
            await _animationSemaphore.WaitAsync(context.CancellationToken);
            semaphoreAcquired = true;
            
            var animationId = $"route-enter-{Interlocked.Increment(ref _animationCounter)}";
            var css = GenerateAnimationCss(animationId, animation.EnterKeyframes, animation);
            
            await InjectAnimationCss(css, context.CancellationToken);
            await ApplyAnimationToElement(component, animationId, context.CancellationToken);
            
            // Wait for animation duration
            await Task.Delay(animation.Duration, context.CancellationToken);
            
            return AnimationResult.Successful(animation.Duration, css);
        }
        catch (OperationCanceledException)
        {
            return AnimationResult.Failed("Animation cancelled");
        }
        catch (Exception ex)
        {
            return AnimationResult.Failed($"Enter animation failed: {ex.Message}");
        }
        finally
        {
            if (semaphoreAcquired)
                _animationSemaphore.Release();
        }
    }

    public void RegisterAnimation(string name, RouteAnimation animation)
    {
        _animations[name] = animation;
    }

    public RouteAnimation? GetAnimation(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        return _animations.TryGetValue(name, out var animation) ? animation : null;
    }

    private RouteAnimation? GetAnimationForComponent(ComponentBase component, RouteAnimationContext context)
    {
        // Check for animation override first
        if (!string.IsNullOrEmpty(context.AnimationOverride))
        {
            return GetAnimation(context.AnimationOverride);
        }

        // Look for RouteAnimationAttribute on component
        var attribute = component.GetType().GetCustomAttribute<RouteAnimationAttribute>();
        if (attribute == null) return null;

        // Create animation from attribute
        var enterAnimation = !string.IsNullOrEmpty(attribute.EnterAnimation) 
            ? GetAnimation(attribute.EnterAnimation)?.EnterKeyframes ?? ""
            : "";
            
        var exitAnimation = !string.IsNullOrEmpty(attribute.ExitAnimation)
            ? GetAnimation(attribute.ExitAnimation)?.ExitKeyframes ?? ""
            : "";

        return new RouteAnimation
        {
            EnterKeyframes = enterAnimation,
            ExitKeyframes = exitAnimation,
            Duration = TimeSpan.FromMilliseconds(attribute.Duration),
            Easing = attribute.Easing,
            EnableViewTransitions = attribute.EnableViewTransitions
        };
    }

    private static string GenerateAnimationCss(string animationId, string keyframes, RouteAnimation animation)
    {
        var easingValue = GetEasingValue(animation.Easing);
        var durationMs = (int)animation.Duration.TotalMilliseconds;
        
        var css = new StringBuilder();
        
        // Generate keyframes
        css.AppendLine($"@keyframes {animationId} {{");
        
        var frames = keyframes.Split("=>", StringSplitOptions.RemoveEmptyEntries);
        if (frames.Length == 2)
        {
            css.AppendLine($"  from {{ {frames[0].Trim()} }}");
            css.AppendLine($"  to {{ {frames[1].Trim()} }}");
        }
        
        css.AppendLine("}");
        
        // Generate animation class
        css.AppendLine($".{animationId} {{");
        css.AppendLine($"  animation: {animationId} {durationMs}ms {easingValue} both;");
        css.AppendLine("}");
        
        return css.ToString();
    }

    private static string GetEasingValue(AnimationEasing easing)
    {
        var field = typeof(AnimationEasing).GetField(easing.ToString());
        var description = field?.GetCustomAttribute<DescriptionAttribute>();
        return description?.Description ?? "ease-in-out";
    }

    private async Task InjectAnimationCss(string css, CancellationToken cancellationToken)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", cancellationToken, 
                $@"
                const style = document.createElement('style');
                style.textContent = `{css}`;
                style.setAttribute('data-modern-router-animation', 'true');
                document.head.appendChild(style);
                setTimeout(() => style.remove(), 1000);
                ");
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected - ignore
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    private async Task ApplyAnimationToElement(ComponentBase component, string animationId, CancellationToken cancellationToken)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", cancellationToken,
                $@"
                const elements = document.querySelectorAll('[data-route-component]');
                if (elements.length > 0) {{
                    const element = elements[elements.length - 1];
                    element.classList.add('{animationId}');
                    setTimeout(() => element.classList.remove('{animationId}'), 1000);
                }}
                ");
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected - ignore
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    private void RegisterBuiltInAnimations()
    {
        // Slide animations
        RegisterAnimation("slideLeft", RouteAnimations.SlideLeft);
        RegisterAnimation("slideRight", RouteAnimations.SlideRight);
        RegisterAnimation("slideUp", RouteAnimations.SlideUp);
        RegisterAnimation("slideDown", RouteAnimations.SlideDown);
        
        // Fade animations
        RegisterAnimation("fadeIn", RouteAnimations.FadeIn);
        RegisterAnimation("fadeOut", RouteAnimations.FadeOut);
        
        // Scale animations
        RegisterAnimation("scaleIn", RouteAnimations.ScaleIn);
        RegisterAnimation("scaleOut", RouteAnimations.ScaleOut);
        
        // Zoom animations
        RegisterAnimation("zoomIn", RouteAnimations.ZoomIn);
        RegisterAnimation("zoomOut", RouteAnimations.ZoomOut);
    }
}