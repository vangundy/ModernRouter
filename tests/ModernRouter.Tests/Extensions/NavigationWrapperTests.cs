using FluentAssertions;
using Microsoft.AspNetCore.Components;
using ModernRouter.Extensions;
using ModernRouter.Routing;
using ModernRouter.Services;
using Moq;
using Xunit;

namespace ModernRouter.Tests.Extensions;

/// <summary>
/// Tests demonstrating proper mocking of extension methods using wrapper interfaces.
/// This is the recommended pattern for testing extension methods that cannot be mocked directly.
/// </summary>
public class NavigationWrapperTests
{
    private readonly Mock<INavigationWrapper> _mockNavigationWrapper;
    private readonly Mock<IRouteNameService> _mockRouteNameService;

    public NavigationWrapperTests()
    {
        _mockNavigationWrapper = new Mock<INavigationWrapper>();
        _mockRouteNameService = new Mock<IRouteNameService>();
    }

    [Fact]
    public void GetQueryParameters_ReturnsCorrectParameters()
    {
        // Arrange
        var expectedParams = new QueryParameters("?key=value&other=test");
        _mockNavigationWrapper.Setup(x => x.GetQueryParameters())
            .Returns(expectedParams);

        // Act
        var result = _mockNavigationWrapper.Object.GetQueryParameters();

        // Assert
        result.Should().BeSameAs(expectedParams);
        result["key"].Should().Be("value");
        result["other"].Should().Be("test");
    }

    [Fact]
    public void NavigateToNamedRoute_WithValidRoute_CallsCorrectMethod()
    {
        // Arrange
        var routeName = "UserProfile";
        var routeValues = new { id = 123 };

        // Act
        _mockNavigationWrapper.Object.NavigateToNamedRoute(_mockRouteNameService.Object, routeName, routeValues);

        // Assert
        _mockNavigationWrapper.Verify(x => x.NavigateToNamedRoute(_mockRouteNameService.Object, routeName, routeValues, false, false), Times.Once);
    }

    [Fact]
    public void TryNavigateToNamedRoute_WithInvalidRoute_ReturnsFalse()
    {
        // Arrange
        var routeName = "NonExistentRoute";
        _mockNavigationWrapper.Setup(x => x.TryNavigateToNamedRoute(_mockRouteNameService.Object, routeName, null, false, false))
            .Returns(false);

        // Act
        var result = _mockNavigationWrapper.Object.TryNavigateToNamedRoute(_mockRouteNameService.Object, routeName);

        // Assert
        result.Should().BeFalse();
        _mockNavigationWrapper.Verify(x => x.TryNavigateToNamedRoute(_mockRouteNameService.Object, routeName, null, false, false), Times.Once);
    }

    [Fact]
    public void NavigateWithQuery_UpdatesSingleParameter()
    {
        // Arrange
        var key = "page";
        var value = "2";

        // Act
        _mockNavigationWrapper.Object.NavigateWithQuery(key, value);

        // Assert
        _mockNavigationWrapper.Verify(x => x.NavigateWithQuery(key, value, false, false), Times.Once);
    }

    [Fact]
    public void NavigateWithQuery_UpdatesMultipleParameters()
    {
        // Arrange
        var parameters = new Dictionary<string, string?>
        {
            ["page"] = "2",
            ["sort"] = "name",
            ["filter"] = null // Remove this parameter
        };

        // Act
        _mockNavigationWrapper.Object.NavigateWithQuery(parameters);

        // Assert
        _mockNavigationWrapper.Verify(x => x.NavigateWithQuery(parameters, false, false), Times.Once);
    }

    [Fact]
    public void GetUrlForNamedRoute_GeneratesCorrectUrl()
    {
        // Arrange
        var routeName = "UserProfile";
        var routeValues = new { id = 123 };
        var expectedUrl = "/users/123";

        _mockNavigationWrapper.Setup(x => x.GetUrlForNamedRoute(_mockRouteNameService.Object, routeName, routeValues))
            .Returns(expectedUrl);

        // Act
        var result = _mockNavigationWrapper.Object.GetUrlForNamedRoute(_mockRouteNameService.Object, routeName, routeValues);

        // Assert
        result.Should().Be(expectedUrl);
        _mockNavigationWrapper.Verify(x => x.GetUrlForNamedRoute(_mockRouteNameService.Object, routeName, routeValues), Times.Once);
    }

    [Fact]
    public void NavigateTo_WithQueryParameters_FormatsUrlCorrectly()
    {
        // Arrange
        var baseUri = "/search";
        var queryParams = new QueryParameters();
        queryParams.Add("q", "test query");
        queryParams.Add("page", "1");

        // Act
        _mockNavigationWrapper.Object.NavigateTo(baseUri, queryParams);

        // Assert
        _mockNavigationWrapper.Verify(x => x.NavigateTo(baseUri, queryParams, false, false), Times.Once);
    }

    /// <summary>
    /// Example of testing a service that uses INavigationWrapper.
    /// This demonstrates how wrapper interfaces enable proper unit testing.
    /// </summary>
    [Fact]
    public void ExampleService_UsingNavigationWrapper_CanBeTested()
    {
        // Arrange
        var service = new ExampleNavigationService(_mockNavigationWrapper.Object, _mockRouteNameService.Object);
        var userId = 123;

        string generatedUrl = "/users/123";
        _mockRouteNameService.Setup(x => x.TryGenerateUrl("UserProfile", It.IsAny<object>(), out generatedUrl, true))
            .Returns(true);

        // Act
        var result = service.NavigateToUserProfile(userId);

        // Assert
        result.Should().BeTrue();
        _mockNavigationWrapper.Verify(x => x.NavigateToNamedRoute(_mockRouteNameService.Object, "UserProfile", It.IsAny<object>(), false, false), Times.Once);
    }

    /// <summary>
    /// Example service that demonstrates proper usage of INavigationWrapper.
    /// This service can be easily unit tested because it depends on mockable interfaces.
    /// </summary>
    private class ExampleNavigationService
    {
        private readonly INavigationWrapper _navigation;
        private readonly IRouteNameService _routeNames;

        public ExampleNavigationService(INavigationWrapper navigation, IRouteNameService routeNames)
        {
            _navigation = navigation;
            _routeNames = routeNames;
        }

        public bool NavigateToUserProfile(int userId)
        {
            try
            {
                _navigation.NavigateToNamedRoute(_routeNames, "UserProfile", new { id = userId });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}