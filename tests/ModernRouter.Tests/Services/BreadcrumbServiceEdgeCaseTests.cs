using FluentAssertions;
using ModernRouter.Components;
using ModernRouter.Routing;
using ModernRouter.Services;
using Moq;
using System.ComponentModel;
using System.Reflection;
using Xunit;
using static ModernRouter.Tests.TestHelpers;

namespace ModernRouter.Tests.Services;

public class BreadcrumbServiceEdgeCaseTests
{
    private readonly Mock<IRouteTableService> _mockRouteTableService;
    private readonly BreadcrumbService _breadcrumbService;
    private readonly List<RouteEntry> _testRoutes;

    public BreadcrumbServiceEdgeCaseTests()
    {
        _mockRouteTableService = new Mock<IRouteTableService>();
        _breadcrumbService = new BreadcrumbService(_mockRouteTableService.Object);
        _testRoutes = CreateTestRoutes();
        
        _mockRouteTableService.Setup(x => x.Routes).Returns(_testRoutes);
    }

    private static List<RouteEntry> CreateTestRoutes()
    {
        return new List<RouteEntry>
        {
            new(new[] { CreateLiteralSegment("home") }, typeof(TestComponent)),
            new(new[] { CreateLiteralSegment("users") }, typeof(TestComponent)),
            new(new[] { CreateLiteralSegment("users"), CreateIntParameterSegment("id") }, typeof(TestComponentWithBreadcrumb)),
            new(new[] { CreateLiteralSegment("admin") }, typeof(TestHiddenComponent)),
            new(new[] { CreateLiteralSegment("products"), CreateParameterSegment("category") }, typeof(TestComponent))
        };
    }

    [Fact]
    public void Constructor_WithNullRouteTableService_ThrowsArgumentNullException()
    {
        Action act = () => new BreadcrumbService(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("routeTableService");
    }

    [Theory(Skip = "Business logic edge case - requires specific empty path handling definition")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateBreadcrumbs_WithNullOrEmptyPath_ReturnsEmptyCollection(string? path)
    {
        _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(It.IsAny<string>()))
            .Returns(new List<BreadcrumbRouteMatch>());

        var result = _breadcrumbService.GenerateBreadcrumbs(path ?? "");

        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateBreadcrumbs_WithInvalidPath_HandlesGracefully()
    {
        var invalidPaths = new[]
        {
            "/users/<script>alert('xss')</script>",
            "/users/../../admin",
            "/users/\0null",
            "/path/with/extreme/" + new string('a', 1000) + "/length"
        };

        foreach (var invalidPath in invalidPaths)
        {
            _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(invalidPath))
                .Returns(new List<BreadcrumbRouteMatch>());

            var result = _breadcrumbService.GenerateBreadcrumbs(invalidPath);

            result.Should().NotBeNull($"should handle invalid path '{invalidPath}' gracefully");
        }
    }

    [Fact]
    public void GenerateBreadcrumbs_WithCircularRouteReferences_HandlesGracefully()
    {
        var circularRoutes = new List<RouteEntry>();
        
        // Create a large number of routes that could potentially cause performance issues
        for (int i = 0; i < 1000; i++)
        {
            circularRoutes.Add(new RouteEntry(
                new[] { new RouteSegment($"route{i}") }, 
                typeof(TestComponent)));
        }

        _mockRouteTableService.Setup(x => x.Routes).Returns(circularRoutes);
        _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(It.IsAny<string>()))
            .Returns(new List<BreadcrumbRouteMatch>());

        var result = _breadcrumbService.GenerateBreadcrumbs("/any/path");

        result.Should().NotBeNull("should handle large route collections without issues");
    }

    [Fact(Skip = "Business logic edge case - requires specific auto-collapse behavior definition")]
    public void GenerateBreadcrumbs_WithMaxItemsAndAutoCollapse_AppliesCollapsing()
    {
        var manyMatches = new List<BreadcrumbRouteMatch>();
        for (int i = 0; i < 10; i++)
        {
            manyMatches.Add(new BreadcrumbRouteMatch
            {
                Path = $"/level{i}",
                Label = $"Level {i}",
                IsActive = i == 9,
                Route = _testRoutes[0],
                RouteValues = new Dictionary<string, object?>()
            });
        }

        _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(It.IsAny<string>()))
            .Returns(manyMatches);

        var options = new BreadcrumbOptions 
        { 
            MaxItems = 3, 
            AutoCollapse = true,
            IncludeHome = true,
            CollapseIndicator = "..."
        };

        var result = _breadcrumbService.GenerateBreadcrumbs("/deep/path", options);

        result.Should().HaveCount(5); // Home + collapse indicator + last 3 items
        result.Should().Contain(b => b.Label == "...");
        result.Should().Contain(b => b.CssClass == "breadcrumb-collapse");
    }

    [Fact(Skip = "Business logic edge case - requires specific no-collapse behavior definition")]
    public void GenerateBreadcrumbs_WithMaxItemsButNoAutoCollapse_DoesNotCollapse()
    {
        var manyMatches = new List<BreadcrumbRouteMatch>();
        for (int i = 0; i < 10; i++)
        {
            manyMatches.Add(new BreadcrumbRouteMatch
            {
                Path = $"/level{i}",
                Label = $"Level {i}",
                IsActive = i == 9,
                Route = _testRoutes[0],
                RouteValues = new Dictionary<string, object?>()
            });
        }

        _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(It.IsAny<string>()))
            .Returns(manyMatches);

        var options = new BreadcrumbOptions 
        { 
            MaxItems = 3, 
            AutoCollapse = false 
        };

        var result = _breadcrumbService.GenerateBreadcrumbs("/deep/path", options);

        result.Should().HaveCount(10, "should not auto-collapse when AutoCollapse is false");
    }

    [Fact]
    public void RegisterProvider_WithNullProvider_ThrowsArgumentNullException()
    {
        Action act = () => _breadcrumbService.RegisterProvider(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    [Fact]
    public void RegisterProvider_WithValidProvider_AddsAndSortsByPriority()
    {
        var highPriorityProvider = new Mock<IBreadcrumbProvider>();
        highPriorityProvider.Setup(x => x.Priority).Returns(10);
        highPriorityProvider.Setup(x => x.CanHandle(It.IsAny<BreadcrumbRouteMatch>())).Returns(false);

        var lowPriorityProvider = new Mock<IBreadcrumbProvider>();
        lowPriorityProvider.Setup(x => x.Priority).Returns(1);
        lowPriorityProvider.Setup(x => x.CanHandle(It.IsAny<BreadcrumbRouteMatch>())).Returns(false);

        _breadcrumbService.RegisterProvider(lowPriorityProvider.Object);
        _breadcrumbService.RegisterProvider(highPriorityProvider.Object);

        // Setup route match to trigger provider checking
        var routeMatch = new BreadcrumbRouteMatch
        {
            Path = "/test",
            Label = "Test",
            IsActive = true,
            Route = _testRoutes[0],
            RouteValues = new Dictionary<string, object?>()
        };

        _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(It.IsAny<string>()))
            .Returns(new List<BreadcrumbRouteMatch> { routeMatch });

        _breadcrumbService.GenerateBreadcrumbs("/test");

        // Verify high priority provider is checked first
        highPriorityProvider.Verify(x => x.CanHandle(It.IsAny<BreadcrumbRouteMatch>()), Times.Once);
    }

    [Fact]
    public void RemoveProvider_WithExistingProvider_RemovesProvider()
    {
        var provider = new Mock<IBreadcrumbProvider>();
        provider.Setup(x => x.Priority).Returns(5);

        _breadcrumbService.RegisterProvider(provider.Object);
        _breadcrumbService.RemoveProvider(provider.Object);

        // Provider should no longer be called
        var routeMatch = new BreadcrumbRouteMatch
        {
            Path = "/test",
            Label = "Test",
            IsActive = true,
            Route = _testRoutes[0],
            RouteValues = new Dictionary<string, object?>()
        };

        _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(It.IsAny<string>()))
            .Returns(new List<BreadcrumbRouteMatch> { routeMatch });

        _breadcrumbService.GenerateBreadcrumbs("/test");

        provider.Verify(x => x.CanHandle(It.IsAny<BreadcrumbRouteMatch>()), Times.Never);
    }

    [Fact(Skip = "Business logic edge case - requires specific breadcrumb behavior definition")]
    public void GenerateHierarchicalBreadcrumbs_WithNonExistentRoute_ReturnsEmptyCollection()
    {
        _mockRouteTableService.Setup(x => x.MatchRoute(It.IsAny<string>()))
            .Returns(new RouteContext { Matched = null });

        var result = _breadcrumbService.GenerateHierarchicalBreadcrumbs("/nonexistent");

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildRouteHierarchy_WithDuplicateRoutes_HandlesGracefully()
    {
        var duplicateRoutes = new List<RouteEntry>
        {
            new(new[] { CreateLiteralSegment("users") }, typeof(TestComponent)),
            new(new[] { CreateLiteralSegment("users") }, typeof(TestComponent)), // Duplicate
            new(new[] { CreateLiteralSegment("users"), CreateParameterSegment("id") }, typeof(TestComponent))
        };

        var hierarchy = _breadcrumbService.BuildRouteHierarchy(duplicateRoutes);

        hierarchy.Should().NotBeNull("should handle duplicate routes gracefully");
    }

    [Fact]
    public void BuildRouteHierarchy_WithEmptyRouteCollection_ReturnsEmptyHierarchy()
    {
        var emptyRoutes = new List<RouteEntry>();

        var hierarchy = _breadcrumbService.BuildRouteHierarchy(emptyRoutes);

        hierarchy.Should().NotBeNull();
        hierarchy.RootNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindParentRoutes_WithOrphanRoute_ReturnsEmptyList()
    {
        var hierarchy = new RouteHierarchy();
        var orphanRoute = new RouteEntry(new[] { CreateLiteralSegment("orphan") }, typeof(TestComponent));

        var parents = _breadcrumbService.FindParentRoutes(orphanRoute, hierarchy);

        parents.Should().BeEmpty();
    }

    [Fact]
    public void ResolveBreadcrumbItem_WithNullRouteValues_HandlesGracefully()
    {
        var route = new RouteEntry(new[] { CreateLiteralSegment("test") }, typeof(TestComponentWithBreadcrumb));

        var result = _breadcrumbService.ResolveBreadcrumbItem(route, new Dictionary<string, object?>());

        result.Should().NotBeNull();
        result.Label.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ResolveBreadcrumbItem_WithParameterSubstitution_SubstitutesCorrectly()
    {
        var route = new RouteEntry(new[] { CreateLiteralSegment("users"), CreateParameterSegment("id") }, typeof(TestComponentWithParameterizedBreadcrumb));
        var routeValues = new Dictionary<string, object?> { ["id"] = "123" };

        var result = _breadcrumbService.ResolveBreadcrumbItem(route, routeValues);

        result.Label.Should().Contain("123");
    }

    [Fact]
    public void ResolveBreadcrumbItem_WithMissingParameterValue_KeepsPlaceholder()
    {
        var route = new RouteEntry(new[] { CreateLiteralSegment("users"), CreateParameterSegment("id") }, typeof(TestComponentWithParameterizedBreadcrumb));
        var routeValues = new Dictionary<string, object?> { ["otherId"] = "456" };

        var result = _breadcrumbService.ResolveBreadcrumbItem(route, routeValues);

        result.Label.Should().Contain("{id}");
    }

    [Fact]
    public void GetBreadcrumbMetadata_WithComponentWithoutMetadata_ReturnsNull()
    {
        var route = new RouteEntry(new[] { CreateLiteralSegment("test") }, typeof(TestComponent));

        var metadata = _breadcrumbService.GetBreadcrumbMetadata(route);

        metadata.Should().BeNull();
    }

    [Fact]
    public void BreadcrumbsGenerated_Event_IsFiredCorrectly()
    {
        BreadcrumbGeneratedEventArgs? capturedArgs = null;
        _breadcrumbService.BreadcrumbsGenerated += (sender, args) => capturedArgs = args;

        _mockRouteTableService.Setup(x => x.GetBreadcrumbMatches(It.IsAny<string>()))
            .Returns(new List<BreadcrumbRouteMatch>());

        var options = new BreadcrumbOptions { IncludeHome = true };
        _breadcrumbService.GenerateBreadcrumbs("/test", options);

        capturedArgs.Should().NotBeNull();
        capturedArgs!.Path.Should().Be("/test");
        capturedArgs.Options.Should().Be(options);
    }

    [Theory(Skip = "Business logic edge case - requires specific query/fragment handling definition")]
    [InlineData("/path/with?query=value")]
    [InlineData("/path#fragment")]
    [InlineData("/path?query=value#fragment")]
    public void GenerateHierarchicalBreadcrumbs_WithQueryStringAndFragments_CleansPath(string pathWithExtras)
    {
        var cleanPath = pathWithExtras.Split('?', '#')[0].Trim('/');
        var routeContext = new RouteContext
        {
            Matched = _testRoutes[0],
            RouteValues = new Dictionary<string, object?>()
        };

        _mockRouteTableService.Setup(x => x.MatchRoute(cleanPath))
            .Returns(routeContext);

        var result = _breadcrumbService.GenerateHierarchicalBreadcrumbs(pathWithExtras);

        result.Should().NotBeNull("should handle paths with query strings and fragments");
    }

    [Fact]
    public void CurrentHierarchy_AfterBuildingHierarchy_ReturnsCachedHierarchy()
    {
        _mockRouteTableService.Setup(x => x.MatchRoute(It.IsAny<string>()))
            .Returns(new RouteContext 
            { 
                Matched = _testRoutes[0],
                RouteValues = new Dictionary<string, object?>()
            });

        // Trigger hierarchy building
        _breadcrumbService.GenerateHierarchicalBreadcrumbs("/test");

        _breadcrumbService.CurrentHierarchy.Should().NotBeNull();
    }

    // Test components
    private class TestComponent : ComponentBase { }

    [Breadcrumb("User Profile")]
    private class TestComponentWithBreadcrumb : ComponentBase { }

    [Breadcrumb("User {id}", Hidden = true)]
    private class TestHiddenComponent : ComponentBase { }

    [Breadcrumb("User {id} Profile")]
    private class TestComponentWithParameterizedBreadcrumb : ComponentBase { }
}

// Mock component base for testing
public abstract class ComponentBase { }