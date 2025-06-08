using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Animations;
using ModernRouter.Extensions;
using ModernRouter.Routing;
using ModernRouter.Services;
using System.Reflection;

namespace ModernRouter.Components;

public partial class Router
{
    [Inject] private IServiceProvider Services { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IRouteTableService RouteTableService { get; set; } = default!;
    [Inject] private IRouteNameService RouteNameService { get; set; } = default!;
    [Parameter] public Assembly AppAssembly { get; set; } = Assembly.GetEntryAssembly()!;
    [Parameter] public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }
    [Parameter] public RenderFragment? NotFound { get; set; }
    [Parameter] public RenderFragment<Exception>? ErrorContent { get; set; }
    [Parameter] public RenderFragment? NavigationProgress { get; set; }

    private List<RouteEntry> _routeTable = [];
    private RouteContext? _current; // Used in Router.razor template for rendering matched route
    private INavMiddleware[] _pipeline = [];
    private bool _isNavigating = false;
    private CancellationTokenSource? _currentNavigationCts;
    private Exception? _navigationError;
    private RouteAnimationContext? _animationContext;

    async protected override Task OnInitializedAsync()
    {
        // Initialize service helper for static access
        ServiceProviderHelper.Initialize(Services);
        
        var assemblies = new List<Assembly> { AppAssembly };
        if (AdditionalAssemblies is not null) assemblies.AddRange(AdditionalAssemblies);
        _routeTable = RouteTableFactory.Build(assemblies);

        // Initialize the route table service
        RouteTableService.Initialize(assemblies);

        // Register named routes
        RegisterNamedRoutes();

        _pipeline = [.. Services.GetServices<INavMiddleware>()];

        await NavigateAsync(Nav.Uri, firstLoad: true);
        Nav.LocationChanged += async (_, e) =>
            await NavigateAsync(e.Location, firstLoad: false);
    }

    private async Task NavigateAsync(string absoluteUri, bool firstLoad)
    {
        // Store the previous URL for potential restoration
        var previousUrl = Nav.Uri;
        
        // Cancel any pending navigation
        if (_currentNavigationCts is not null)
        {
#pragma warning disable S6966 // Awaitable method should be used
            _currentNavigationCts.Cancel();
#pragma warning restore S6966 // Awaitable method should be used
            _currentNavigationCts.Dispose();
            _currentNavigationCts = null;
        }
        _currentNavigationCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Show progress indicator
        _isNavigating = !firstLoad;
        StateHasChanged();

        var relative = Nav.ToBaseRelativePath(absoluteUri);
        var match = RouteMatcher.Match(_routeTable, relative);

        // Create animation context for route transitions
        _animationContext = new RouteAnimationContext
        {
            FromRoute = _current?.Matched?.TemplateString ?? "",
            ToRoute = match.Matched?.TemplateString ?? "",
            RouteValues = match.RouteValues,
            NavigationType = firstLoad ? NavigationType.Push : NavigationType.Push, // Could be enhanced to detect back/forward
            CancellationToken = _currentNavigationCts.Token
        };

        NavContext navContext = new()
        {
            TargetUri = absoluteUri,
            Match = match,
            CancellationToken = _currentNavigationCts.Token
        };
        try
        {
            // Clear any previous navigation error
            _navigationError = null;
            
            // Check for cancellation before starting
            _currentNavigationCts.Token.ThrowIfCancellationRequested();
            
            var result = await InvokePipelineAsync(navContext, 0);

            // Final cancellation check after pipeline
            _currentNavigationCts.Token.ThrowIfCancellationRequested();

            if (result.Type == NavResultType.Cancel)
            {
                // Restore browser URL when cancelled (except initial load)
                if (!firstLoad) Nav.NavigateTo(previousUrl, forceLoad: false, replace: true);
                return;
            }
            if (result.Type == NavResultType.Error)
            {
                // Store the error for display in the UI
                _navigationError = result.Exception ?? new Exception("Navigation pipeline error occurred");
                // Also restore URL on error
                if (!firstLoad) Nav.NavigateTo(previousUrl, forceLoad: false, replace: true);
                return;
            }
            if (result.RedirectUrl is not null)
            {
                Nav.NavigateTo(result.RedirectUrl, forceLoad: false);
                return;
            }

            // Handle alias redirects to primary route
            if (match.IsAliasMatch && match.MatchedAlias?.RedirectToPrimary is true && match.Matched != null)
            {
                Console.WriteLine($"Debug: Alias match detected for redirect. Alias: {match.MatchedAlias.TemplateString}, Primary: {match.Matched?.TemplateString}");
                Console.WriteLine($"Debug: RouteValues: {string.Join(", ", match.RouteValues.Select(kv => $"{kv.Key}={kv.Value}"))}");
                
                // Generate the primary route URL with the same parameters
                var primaryUrl = GeneratePrimaryRouteUrl(match);
                Console.WriteLine($"Debug: Generated primary URL: {primaryUrl}");
                
                if (!string.IsNullOrEmpty(primaryUrl))
                {
                    Console.WriteLine($"Debug: Redirecting to primary URL: {primaryUrl}");
                    Nav.NavigateTo(primaryUrl, forceLoad: false, replace: true);
                    return;
                }
                else
                {
                    Console.WriteLine("Debug: Failed to generate primary URL for redirect");
                }
            }

            _current = match;
        }
        catch (OperationCanceledException)
        {
            // Navigation was cancelled - restore previous URL
            if (!firstLoad) Nav.NavigateTo(previousUrl, forceLoad: false, replace: true);
        }
        catch (Exception ex)
        {
            // Handle any unhandled exceptions from the pipeline
            _navigationError = ex;
            // Restore URL on unhandled error
            if (!firstLoad) Nav.NavigateTo(previousUrl, forceLoad: false, replace: true);
        }
        finally
        {
            // Only clear navigation state if this is still the current navigation
            if (_currentNavigationCts?.Token == navContext.CancellationToken)
            {
                // Hide progress indicator
                _isNavigating = false;
                StateHasChanged();

                // Dispose the token source only after we're done with the navigation
                var cts = _currentNavigationCts;
                _currentNavigationCts = null;
                cts.Dispose();
            }
        }
    }

    private async Task<NavResult> InvokePipelineAsync(NavContext navContext, int index)
    {
        // Check for cancellation before each middleware
        navContext.CancellationToken.ThrowIfCancellationRequested();
        
        if (index == _pipeline.Length)
            return NavResult.Allow();

        try
        {
            return await _pipeline[index].InvokeAsync(navContext, () => InvokePipelineAsync(navContext, index + 1));
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation to be handled at the navigation level
            throw;
        }
        catch (Exception ex)
        {
            // Convert unhandled middleware exceptions to NavResult.Error
            return NavResult.Error(ex);
        }
    }

    private void RegisterNamedRoutes()
    {
        // Clear any existing named routes
        RouteNameService.Clear();

        // Register routes that have names
        foreach (var route in _routeTable.Where(r => !string.IsNullOrEmpty(r.Name)))
        {
            RouteNameService.RegisterRoute(route.Name!, route);
        }
    }

    private static string GeneratePrimaryRouteUrl(RouteContext routeContext)
    {
        if (routeContext.Matched == null || !routeContext.IsAliasMatch)
            return string.Empty;

        try
        {
            var primaryTemplate = routeContext.Matched.TemplateString;
            
            // Ensure the primary template starts with a slash
            if (!primaryTemplate.StartsWith('/'))
                primaryTemplate = "/" + primaryTemplate;
            
            Console.WriteLine($"Debug: RouteValues before URL generation: {string.Join(", ", routeContext.RouteValues.Select(kv => $"{kv.Key}={kv.Value}"))}");
            
            // Replace parameter placeholders with actual values
            string result = primaryTemplate;
            foreach (var param in routeContext.RouteValues)
            {
                string placeholder = $"{{{param.Key}}}";
                string placeholder2 = $"{{{param.Key}:int}}"; // Handle common constraints
                
                if (param.Value != null)
                {
                    string valueStr = param.Value.ToString() ?? string.Empty;
                    result = result.Replace(placeholder, valueStr, StringComparison.OrdinalIgnoreCase);
                    result = result.Replace(placeholder2, valueStr, StringComparison.OrdinalIgnoreCase);
                }
            }
            
            Console.WriteLine($"Debug: Generated URL after replacement: {result}");
            
            // Add query parameters if there are any
            if (routeContext.QueryParameters.Count > 0)
            {
                result += routeContext.QueryParameters.ToString();
            }
            
            return result;
        }
        catch (Exception ex)
        {
            // Log the error but don't fail navigation
            Console.WriteLine($"Warning: Failed to generate primary route URL: {ex.Message}");
            return string.Empty;
        }
    }
}