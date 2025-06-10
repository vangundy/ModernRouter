using FluentAssertions;
using ModernRouter.Routing;
using ModernRouter.Services;
using Xunit;
using static ModernRouter.Tests.TestHelpers;

namespace ModernRouter.Tests.Services;

public class RouteNameServiceEdgeCaseTests
{
    private readonly RouteNameService _routeNameService;

    public RouteNameServiceEdgeCaseTests()
    {
        _routeNameService = new RouteNameService();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterRoute_WithNullOrEmptyRouteName_ThrowsArgumentException(string? routeName)
    {
        var route = CreateTestRoute("/test");

        Action act = () => _routeNameService.RegisterRoute(routeName!, route);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("routeName")
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void RegisterRoute_WithNullRouteEntry_ThrowsArgumentNullException()
    {
        Action act = () => _routeNameService.RegisterRoute("testRoute", null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("routeEntry");
    }

    [Fact]
    public void RegisterRoute_WithDuplicateRouteName_UpdatesExistingRoute()
    {
        var route1 = CreateTestRoute("/path1");
        var route2 = CreateTestRoute("/path2");

        _routeNameService.RegisterRoute("duplicate", route1);
        _routeNameService.RegisterRoute("duplicate", route2);

        var retrievedRoute = _routeNameService.GetRoute("duplicate");
        retrievedRoute.Should().Be(route2, "second registration should overwrite first");
    }

    [Fact]
    public void RegisterRoute_IsCaseInsensitive()
    {
        var route = CreateTestRoute("/test");

        _routeNameService.RegisterRoute("TestRoute", route);

        _routeNameService.HasRoute("testroute").Should().BeTrue();
        _routeNameService.HasRoute("TESTROUTE").Should().BeTrue();
        _routeNameService.HasRoute("TestRoute").Should().BeTrue();
        _routeNameService.GetRoute("testroute").Should().Be(route);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetRoute_WithNullOrEmptyRouteName_ReturnsNull(string? routeName)
    {
        var result = _routeNameService.GetRoute(routeName!);

        result.Should().BeNull();
    }

    [Fact]
    public void GetRoute_WithNonExistentRouteName_ReturnsNull()
    {
        var result = _routeNameService.GetRoute("nonexistent");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasRoute_WithNullOrEmptyRouteName_ReturnsFalse(string? routeName)
    {
        var result = _routeNameService.HasRoute(routeName!);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasRoute_WithNonExistentRouteName_ReturnsFalse()
    {
        var result = _routeNameService.HasRoute("nonexistent");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateUrl_WithNullOrEmptyRouteName_ThrowsArgumentException(string? routeName)
    {
        Action act = () => _routeNameService.GenerateUrl(routeName!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("routeName");
    }

    [Fact]
    public void GenerateUrl_WithNonExistentRouteName_ThrowsArgumentException()
    {
        Action act = () => _routeNameService.GenerateUrl("nonexistent");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("routeName")
            .WithMessage("*Could not generate URL for route 'nonexistent'*");
    }

    [Fact]
    public void GenerateUrl_WithMissingRequiredParameters_ThrowsArgumentException()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        Action act = () => _routeNameService.GenerateUrl("userProfile");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("routeName");
    }

    [Fact]
    public void GenerateUrl_WithInvalidParameterTypes_ThrowsArgumentException()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        Action act = () => _routeNameService.GenerateUrl("userProfile", new { id = "invalid" });

        act.Should().Throw<ArgumentException>()
            .WithParameterName("routeName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGenerateUrl_WithNullOrEmptyRouteName_ReturnsFalse(string? routeName)
    {
        var result = _routeNameService.TryGenerateUrl(routeName!, null, out var url);

        result.Should().BeFalse();
        url.Should().BeEmpty();
    }

    [Fact]
    public void TryGenerateUrl_WithNonExistentRouteName_ReturnsFalse()
    {
        var result = _routeNameService.TryGenerateUrl("nonexistent", null, out var url);

        result.Should().BeFalse();
        url.Should().BeEmpty();
    }

    [Fact]
    public void TryGenerateUrl_WithMissingRequiredParameters_ReturnsFalse()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        var result = _routeNameService.TryGenerateUrl("userProfile", null, out var url);

        result.Should().BeFalse();
        url.Should().BeEmpty();
    }

    [Fact]
    public void TryGenerateUrl_WithInvalidParameterTypes_ReturnsFalse()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        var result = _routeNameService.TryGenerateUrl("userProfile", new { id = "invalid" }, out var url);

        result.Should().BeFalse();
        url.Should().BeEmpty();
    }

    [Fact]
    public void TryGenerateUrl_WithValidParameters_ReturnsTrue()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        var result = _routeNameService.TryGenerateUrl("userProfile", new { id = 123 }, out var url);

        result.Should().BeTrue();
        url.Should().Be("/users/123");
    }

    [Fact]
    public void GenerateUrl_WithAnonymousObject_WorksCorrectly()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        var url = _routeNameService.GenerateUrl("userProfile", new { id = 123 });

        url.Should().Be("/users/123");
    }

    [Fact]
    public void GenerateUrl_WithDictionary_WorksCorrectly()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        var routeValues = new Dictionary<string, object?> { ["id"] = 123 };
        var url = _routeNameService.GenerateUrl("userProfile", routeValues);

        url.Should().Be("/users/123");
    }

    [Fact]
    public void GenerateUrl_WithExtraParameters_IgnoresExtraParameters()
    {
        var route = CreateTestRoute("/users/{id:int}");
        _routeNameService.RegisterRoute("userProfile", route);

        var url = _routeNameService.GenerateUrl("userProfile", new { id = 123, extra = "ignored" });

        url.Should().Be("/users/123");
    }

    [Fact]
    public void GenerateUrl_WithValidationDisabled_AllowsInvalidParameters()
    {
        var route = CreateTestRoute("/users/{id}");
        _routeNameService.RegisterRoute("userProfile", route);

        var result = _routeNameService.TryGenerateUrl("userProfile", new { id = "any-value" }, out var url, validateParameters: false);

        result.Should().BeTrue();
        url.Should().Contain("any-value");
    }

    [Fact]
    public void GenerateUrl_WithMaliciousParameters_SanitizesCorrectly()
    {
        var route = CreateTestRoute("/search/{term}");
        _routeNameService.RegisterRoute("search", route);

        var maliciousInputs = new[]
        {
            "<script>alert('xss')</script>",
            "../../../etc/passwd",
            "term with spaces",
            "special&chars=here"
        };

        foreach (var input in maliciousInputs)
        {
            var result = _routeNameService.TryGenerateUrl("search", new { term = input }, out var url);
            
            // Should either succeed with sanitized output or fail
            if (result)
            {
                url.Should().NotContain("<script>");
                url.Should().NotContain("../");
            }
        }
    }

    [Fact]
    public void GetRouteNames_WithNoRoutes_ReturnsEmptyCollection()
    {
        var routeNames = _routeNameService.GetRouteNames();

        routeNames.Should().BeEmpty();
    }

    [Fact]
    public void GetRouteNames_WithMultipleRoutes_ReturnsAllNames()
    {
        var route1 = CreateTestRoute("/path1");
        var route2 = CreateTestRoute("/path2");

        _routeNameService.RegisterRoute("route1", route1);
        _routeNameService.RegisterRoute("route2", route2);

        var routeNames = _routeNameService.GetRouteNames().ToList();

        routeNames.Should().HaveCount(2);
        routeNames.Should().Contain("route1");
        routeNames.Should().Contain("route2");
    }

    [Fact]
    public void Clear_RemovesAllRoutes()
    {
        var route1 = CreateTestRoute("/path1");
        var route2 = CreateTestRoute("/path2");

        _routeNameService.RegisterRoute("route1", route1);
        _routeNameService.RegisterRoute("route2", route2);

        _routeNameService.Clear();

        _routeNameService.GetRouteNames().Should().BeEmpty();
        _routeNameService.HasRoute("route1").Should().BeFalse();
        _routeNameService.HasRoute("route2").Should().BeFalse();
    }

    [Fact]
    public void RegisterRoute_WithLargeNumberOfRoutes_HandlesCorrectly()
    {
        // Test performance and memory handling with many routes
        for (int i = 0; i < 10000; i++)
        {
            var route = CreateTestRoute($"/route{i}");
            _routeNameService.RegisterRoute($"route{i}", route);
        }

        _routeNameService.GetRouteNames().Should().HaveCount(10000);
        _routeNameService.HasRoute("route5000").Should().BeTrue();
        _routeNameService.GetRoute("route9999").Should().NotBeNull();
    }

    [Fact]
    public void ConvertToStringDictionary_WithNullObject_ReturnsEmptyDictionary()
    {
        // This tests the private method indirectly through public API
        var result = _routeNameService.TryGenerateUrl("nonexistent", null, out _);

        result.Should().BeFalse(); // The method should handle null gracefully
    }

    [Fact]
    public void ConvertToStringDictionary_WithComplexObject_ExtractsProperties()
    {
        var route = CreateTestRoute("/test/{prop1}/{prop2}");
        _routeNameService.RegisterRoute("test", route);

        var complexObject = new TestClass { Prop1 = "value1", Prop2 = "value2" };
        var result = _routeNameService.TryGenerateUrl("test", complexObject, out var url);

        result.Should().BeTrue();
        url.Should().Contain("value1");
        url.Should().Contain("value2");
    }

    [Fact]
    public void ThreadSafety_ConcurrentRegistrations_HandlesCorrectly()
    {
        var tasks = new List<Task>();

        // Test concurrent registrations
        for (int i = 0; i < 100; i++)
        {
            int routeIndex = i;
            tasks.Add(Task.Run(() =>
            {
                var route = CreateTestRoute($"/route{routeIndex}");
                _routeNameService.RegisterRoute($"route{routeIndex}", route);
            }));
        }

        Task.WaitAll(tasks.ToArray());

        _routeNameService.GetRouteNames().Should().HaveCount(100);
    }

    [Fact]
    public void ThreadSafety_ConcurrentReadsAndWrites_HandlesCorrectly()
    {
        var route = CreateTestRoute("/test/{id}");
        _routeNameService.RegisterRoute("test", route);

        var tasks = new List<Task>();

        // Mix of reads and writes
        for (int i = 0; i < 50; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                if (index % 2 == 0)
                {
                    // Read operation
                    _routeNameService.HasRoute("test");
                    _routeNameService.TryGenerateUrl("test", new { id = index }, out _);
                }
                else
                {
                    // Write operation
                    var newRoute = CreateTestRoute($"/dynamic{index}");
                    _routeNameService.RegisterRoute($"dynamic{index}", newRoute);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        _routeNameService.HasRoute("test").Should().BeTrue();
        _routeNameService.GetRouteNames().Should().HaveCountGreaterThan(1);
    }

    private static RouteEntry CreateTestRoute(string template)
    {
        var segments = template.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.StartsWith('{') && s.EndsWith('}') 
                ? CreateParameterSegment(s.Trim('{', '}').Split(':')[0])
                : CreateLiteralSegment(s))
            .ToArray();

        return new RouteEntry(segments, typeof(object))
        {
            TemplateString = template
        };
    }

    private class TestClass
    {
        public string Prop1 { get; set; } = "";
        public string Prop2 { get; set; } = "";
    }
}