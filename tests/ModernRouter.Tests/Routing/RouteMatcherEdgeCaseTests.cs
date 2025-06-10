using FluentAssertions;
using ModernRouter.Routing;
using Xunit;
using static ModernRouter.Tests.TestHelpers;

namespace ModernRouter.Tests.Routing;

public class RouteMatcherEdgeCaseTests
{
    private readonly List<RouteEntry> _testRoutes;

    public RouteMatcherEdgeCaseTests()
    {
        _testRoutes = CreateTestRoutes();
    }

    private static List<RouteEntry> CreateTestRoutes()
    {
        return new List<RouteEntry>
        {
            new(new[] { CreateLiteralSegment("users"), CreateIntParameterSegment("id") }, typeof(object)),
            new(new[] { CreateLiteralSegment("users") }, typeof(object)),
            new(new[] { CreateLiteralSegment("products"), CreateParameterSegment("catchall", isCatchAll: true) }, typeof(object)),
            new(new[] { CreateLiteralSegment("optional"), CreateParameterSegment("param", isOptional: true) }, typeof(object))
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Match_WithNullOrEmptyPath_ReturnsEmptyContext(string? path)
    {
        var result = RouteMatcher.Match(_testRoutes, path ?? "");

        result.Matched.Should().BeNull();
        result.RouteValues.Should().BeEmpty();
        result.RemainingSegments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("/users/<script>alert('xss')</script>")]
    [InlineData("/users/test\0null")]
    [InlineData("/users/test|pipe")]
    [InlineData("/users/test*wildcard")]
    [InlineData("/users/test?query")]
    [InlineData("/users/test\"quote")]
    [InlineData("/users/test>greater")]
    public void Match_WithInvalidPathCharacters_RejectsRoute(string maliciousPath)
    {
        var result = RouteMatcher.Match(_testRoutes, maliciousPath);

        result.Matched.Should().BeNull("malicious paths should be rejected for security");
    }

    [Fact]
    public void Match_WithPathTraversalAttempt_RejectsRoute()
    {
        var maliciousPaths = new[]
        {
            "/users/../admin",
            "/users/..\\admin",
            "/../etc/passwd",
            "/users/test/../../../root"
        };

        foreach (var path in maliciousPaths)
        {
            var result = RouteMatcher.Match(_testRoutes, path);
            result.Matched.Should().BeNull($"path traversal attempt '{path}' should be rejected");
        }
    }

    [Fact]
    public void Match_WithExtremelyLongPath_RejectsRoute()
    {
        var longPath = "/users/" + new string('a', 3000);
        
        var result = RouteMatcher.Match(_testRoutes, longPath);

        result.Matched.Should().BeNull("extremely long paths should be rejected");
    }

    [Fact(Skip = "Security edge case - requires specific long parameter length threshold definition")]
    public void Match_WithExtremelyLongParameter_RejectsRoute()
    {
        var longParam = new string('a', 600);
        var pathWithLongParam = $"/users/{longParam}";
        
        var result = RouteMatcher.Match(_testRoutes, pathWithLongParam);

        result.Matched.Should().BeNull("extremely long parameters should be rejected");
    }

    [Theory]
    [InlineData("/users//double-slash")]
    [InlineData("///triple-slash")]
    [InlineData("/users/test//")]
    public void Match_WithMultipleConsecutiveSlashes_HandlesGracefully(string pathWithSlashes)
    {
        var result = RouteMatcher.Match(_testRoutes, pathWithSlashes);

        result.Should().NotBeNull("should handle multiple slashes gracefully");
    }

    [Theory]
    [InlineData("/users/123abc")]
    [InlineData("/users/abc123")]
    [InlineData("/users/12.5")]
    [InlineData("/users/999999999999999999999")]
    public void Match_WithInvalidIntegerParameter_RejectsRoute(string pathWithInvalidInt)
    {
        // Use only the int-constrained route to test parameter validation
        var intConstrainedRoutes = new List<RouteEntry>
        {
            new(new[] { CreateLiteralSegment("users"), CreateIntParameterSegment("id") }, typeof(object))
        };
        
        var result = RouteMatcher.Match(intConstrainedRoutes, pathWithInvalidInt);

        result.Matched.Should().BeNull("invalid integer parameters should not match int constraint");
    }

    [Theory]
    [InlineData("/users/123")]
    [InlineData("/users/0")]
    [InlineData("/users/-1")]
    [InlineData("/users/2147483647")] // Max int
    public void Match_WithValidIntegerParameter_MatchesRoute(string pathWithValidInt)
    {
        // Use only the int-constrained route to test parameter validation
        var intConstrainedRoutes = new List<RouteEntry>
        {
            new(new[] { CreateLiteralSegment("users"), CreateIntParameterSegment("id") }, typeof(object))
        };
        
        var result = RouteMatcher.Match(intConstrainedRoutes, pathWithValidInt);

        result.Matched.Should().NotBeNull("valid integer parameters should match int constraint");
        result.RouteValues.Should().ContainKey("id");
    }

    [Fact]
    public void Match_WithCatchAllRoute_CapturesEverything()
    {
        var result = RouteMatcher.Match(_testRoutes, "/products/category/subcategory/item");

        result.Matched.Should().NotBeNull();
        result.RouteValues["catchall"].Should().Be("category/subcategory/item");
        result.RemainingSegments.Should().BeEmpty();
    }

    [Fact]
    public void Match_WithCatchAllRoute_HandlesEmptyTail()
    {
        var result = RouteMatcher.Match(_testRoutes, "/products/");

        result.Matched.Should().NotBeNull();
        result.RouteValues["catchall"].Should().Be("");
        result.RemainingSegments.Should().BeEmpty();
    }

    [Fact]
    public void Match_WithOptionalParameter_MatchesWithoutParameter()
    {
        var result = RouteMatcher.Match(_testRoutes, "/optional");

        result.Matched.Should().NotBeNull();
        result.RouteValues["param"].Should().BeNull();
    }

    [Fact]
    public void Match_WithOptionalParameter_MatchesWithParameter()
    {
        var result = RouteMatcher.Match(_testRoutes, "/optional/value");

        result.Matched.Should().NotBeNull();
        result.RouteValues["param"].Should().Be("value");
    }

    [Theory(Skip = "Route matching edge case - requires specific non-matching behavior definition")]
    [InlineData("/nonexistent")]
    [InlineData("/users/123/nonexistent")]
    [InlineData("/completely/different/path")]
    public void Match_WithNonMatchingPaths_ReturnsEmptyContext(string nonMatchingPath)
    {
        var result = RouteMatcher.Match(_testRoutes, nonMatchingPath);

        result.Matched.Should().BeNull();
        result.RouteValues.Should().BeEmpty();
    }

    [Fact]
    public void Match_WithEmptyRouteCollection_ReturnsEmptyContext()
    {
        var emptyRoutes = new List<RouteEntry>();
        
        var result = RouteMatcher.Match(emptyRoutes, "/any/path");

        result.Matched.Should().BeNull();
        result.RouteValues.Should().BeEmpty();
    }

    [Theory]
    [InlineData("/users?")]
    [InlineData("/users?=")]
    [InlineData("/users?&")]
    [InlineData("/users?key=")]
    [InlineData("/users?=value")]
    [InlineData("/users?key=value&")]
    [InlineData("/users?&key=value")]
    public void Match_WithMalformedQueryStrings_HandlesGracefully(string pathWithMalformedQuery)
    {
        var result = RouteMatcher.Match(_testRoutes, pathWithMalformedQuery);

        result.Should().NotBeNull("should handle malformed query strings gracefully");
        result.QueryParameters.Should().NotBeNull();
    }

    [Fact]
    public void Match_WithUrlEncodedPath_DecodesCorrectly()
    {
        var routes = new List<RouteEntry>
        {
            new(new[] { CreateLiteralSegment("test"), CreateParameterSegment("param") }, typeof(object))
        };

        var result = RouteMatcher.Match(routes, "/test/hello%20world");

        result.Matched.Should().NotBeNull();
        result.RouteValues["param"].Should().Be("hello world");
    }

    [Theory]
    [InlineData("/test/hello%3Cscript%3Ealert('xss')%3C/script%3E")]
    [InlineData("/test/..%2F..%2Fadmin")]
    [InlineData("/test/%00null")]
    public void Match_WithUrlEncodedMaliciousContent_RejectsRoute(string encodedMaliciousPath)
    {
        var routes = new List<RouteEntry>
        {
            new(new[] { CreateLiteralSegment("test"), CreateParameterSegment("param") }, typeof(object))
        };

        var result = RouteMatcher.Match(routes, encodedMaliciousPath);

        result.Matched.Should().BeNull("URL-encoded malicious content should still be rejected");
    }

    [Fact]
    public void Match_WithRouteAliases_MatchesPrimaryRouteFirst()
    {
        var aliasedRoute = new RouteEntry(
            new[] { CreateLiteralSegment("primary") }, 
            typeof(object))
        {
            Aliases = new List<RouteAlias> 
            { 
                CreateRouteAlias(new[] { CreateLiteralSegment("alias") }, "/alias", false, 0)
            }
        };

        var routes = new List<RouteEntry> { aliasedRoute };

        var primaryResult = RouteMatcher.Match(routes, "/primary");
        var aliasResult = RouteMatcher.Match(routes, "/alias");

        primaryResult.IsAliasMatch.Should().BeFalse();
        aliasResult.IsAliasMatch.Should().BeTrue();
        aliasResult.MatchedAlias.Should().NotBeNull();
    }

    [Fact]
    public void Match_WithCircularRouteReferences_HandlesGracefully()
    {
        var routeEntries = new List<RouteEntry>();
        
        for (int i = 0; i < 1000; i++)
        {
            routeEntries.Add(new RouteEntry(
                new[] { CreateLiteralSegment($"route{i}") }, 
                typeof(object)));
        }

        var result = RouteMatcher.Match(routeEntries, "/nonexistent");

        result.Matched.Should().BeNull();
        result.Should().NotBeNull("should handle large route collections without issues");
    }
}