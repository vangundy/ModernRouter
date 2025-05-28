# ModernRouter

A powerful hierarchical routing library for Blazor WebAssembly applications built with .NET 9 and C# 13.0.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download)

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Examples](#examples)
- [Architecture](#architecture)
- [Middleware Guards](#middleware-guards)
- [Navigation Results](#navigation-results)
- [Data Loaders](#data-loaders)
- [Error Boundaries](#error-boundaries)
- [Route Metadata and Authorization](#route-metadata-and-authorization)
- [Implementing Authorization Service](#implementing-authorization-service)
- [Technical Requirements](#technical-requirements)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

## Overview

ModernRouter provides an advanced, hierarchical routing system designed to handle complex routing scenarios in modern Blazor WebAssembly applications.

## Features

### Advanced Route Parameter Handling
- **Type-Constrained Parameters**: Specify data types for route parameters (int, guid, datetime, etc.)
- **Optional Parameters**: Define parameters that don't need to be included in the URL
- **Catch-All Parameters**: Capture multiple URL segments with a single parameter
- **Strong Typing**: Route parameters are automatically converted to their specified types

### Hierarchical Routing with Outlets
- **Nested Routes**: Support for parent-child relationships between routes
- **Component Outlets**: Place child components at specific locations within parent components
- **Context Preservation**: Route data and parameters are cascaded through the component hierarchy
- **Remaining Segment Handling**: Unmatched segments are passed to child outlets

### Intelligent Route Matching
- **Priority-Based Matching**: Routes are matched in order of specificity
- **Type Validation**: Parameter constraints are enforced during matching
- **Case-Insensitive Matching**: Route segments match regardless of case
- **Flexible Template Structure**: Support for complex route patterns

### Navigation Middleware Guards
- Intercept and control navigation requests using middleware
- Implement authentication, analytics, unsaved changes prompts, and more
- Middleware are registered as services and executed in order
- Each middleware can allow, redirect, or cancel navigation

### Async Data Loading
- Supports async, DI-aware, and cancellable data loaders for routed components
- Loader runs after navigation guards but before component rendering
- Loader result is cascaded to the component and all its descendants via [CascadingParameter]
- Enables efficient data fetching and delivery for each route and nested outlet

## Installation

To install ModernRouter, add the library to your Blazor WebAssembly project using NuGet:

`dotnet add package ModernRouter`

## Quick Start

1. Add the ModernRouter library to your Blazor WebAssembly project.
2. Register ModernRouter services in your `Program.cs`:
```csharp
// basic setup
builder.Services.AddModernRouter();
// Or with authorization support 
builder.Services.AddModernRouterWithAuthorization(options => 
{ 
    options.LoginPath = "/auth/login"; 
    options.ForbiddenPath = "/unauthorized"; 
});
```
3. Set up your main router in `App.razor`:
```razor
<ModernRouter.Components.Router AppAssembly="@typeof(Program).Assembly"> 
    <NotFound> 
        <h1>Page not found</h1> 
    </NotFound> 
    <ErrorContent Context="exception"> 
        <div class="error-container"> 
            <h2>An error occurred</h2> 
            <p>@exception.Message</p> 
        </div> 
    </ErrorContent> 
</ModernRouter.Components.Router>
```
4. Define routes using page directives on your components.
5. Place `Outlet` components where nested content should appear.
6. Use strongly-typed parameters in your components.
7. Register any navigation middleware guards as needed.
8. Implement and attach data loaders as needed.

## Examples

### Basic Route Component

```razor
@page "/products" @page "/products/{Category}"
<h1>Products @(Category ?? "All Categories")</h1>
@code { 
    [Parameter] public string? Category { get; set; } 
}
```

### Using Nested Routes

```razor
@page "/admin"
<h1>Admin Dashboard</h1>
<nav> 
    <NavLink href="/admin/users">Users</NavLink> 
    <NavLink href="/admin/settings">Settings</NavLink> 
</nav>
<Outlet />

```

### Master-Detail Views

Create master-detail views with independent routing for each section.

### Tabbed Interfaces

Implement tabbed interfaces with URL-based navigation.

### Wizard-Like Flows

Build wizard-like flows with URL history support.

### Deep-Linking

Enable deep-linking to specific application states.

### Async Data Fetching

Fetch data asynchronously for each route and nested outlet.

## Architecture

ModernRouter uses a component-based architecture with these key parts:

1. **Router**: The main router component that initiates the routing process.
2. **RouteTableFactory**: Builds a routing table by analyzing component route attributes.
3. **RouteMatcher**: Matches URL paths against route templates.
4. **Outlet**: Renders matched components at specific locations in the component hierarchy.
5. **RouteContext**: Encapsulates routing state and parameters.
6. **INavMiddleware**: Interface for navigation middleware guards.
7. **IRouteDataLoader**: Interface for async data loaders.

## Middleware Guards

ModernRouter supports a middleware pipeline for navigation, allowing you to add cross-cutting concerns to routing. Middleware guards implement the `INavMiddleware` interface and can:
- Inspect or modify navigation context.
- Allow, redirect, or cancel navigation.
- Be chained in a configurable order.

### Common Middleware Examples

- **AuthGuard**: Restricts access to certain routes based on authentication state.
- **UnsavedGuard**: Prompts users about unsaved changes before navigating away.
- **AnalyticsTap**: Tracks page views or navigation events for analytics.

To register middleware guards, add them to the DI container in your `Program.cs`:
```csharp
builder.Services.AddScoped<INavMiddleware, AnalyticsTap>();
builder.Services.AddScoped<INavMiddleware, AuthGuard>();
builder.Services.AddScoped<INavMiddleware, UnsavedGuard>();
```
### Middleware Execution Order

In ModernRouter, middleware guards are executed in the same order they're registered in the dependency injection container. This execution order is critical to the proper functioning of your application.

#### How Execution Order Works

1. Middleware are executed sequentially based on registration order
2. Each middleware receives a `next` delegate that represents the remainder of the pipeline
3. A middleware can choose to:
   - Call `next()` to continue the pipeline
   - Return a result without calling `next()` to short-circuit execution

#### Example: Middleware Execution Flow

Registration order:
1. LoggingMiddleware
2. AuthGuard
3. UnsavedGuard

Execution flow: 

    → LoggingMiddleware (logs request) 
    → AuthGuard (checks authentication) 
    → UnsavedGuard (checks unsaved changes)
    
    → RouteView rendering 

    ← UnsavedGuard (completes) 
    ← AuthGuard (completes) 
    ← LoggingMiddleware (logs completion)

#### Best Practices for Middleware Order

Order your middleware based on their function:

1. **Diagnostic middleware** (logging, metrics) should come first to capture all navigation attempts
2. **Authentication/authorization middleware** should come early to prevent unnecessary processing
3. **State preservation middleware** (unsaved changes) should come after auth but before expensive operations
4. **Validation middleware** that might redirect should come after state preservation
5. **Analytics middleware** often works best at the end to capture only successful navigations

#### Example: Recommended Registration Order

```csharp
// Recommended order in Program.cs

// First - logs everything 
builder.Services.AddScoped<INavMiddleware, LoggingMiddleware>();  
// Early - prevents unauthorized access 
builder.Services.AddScoped<INavMiddleware, AuthGuard>();         
// Middle - preserves user warnings
builder.Services.AddScoped<INavMiddleware, UnsavedGuard>();      
```

If your middleware needs to execute in a specific order regardless of registration order, consider implementing an `Order` property in your middleware interface and sorting them before execution.

## Navigation Results

When implementing middleware guards, your middleware returns a `NavResult` that controls how navigation proceeds:

### NavResult Types

- **Allow**: Permits navigation to continue to the requested route
- **Redirect**: Redirects navigation to a different URL
- **Cancel**: Prevents navigation from continuing
- **Error**: Indicates an error occurred during navigation

### Code Examples

```csharp
// Allow navigation to continue 
public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next) 
{ 
    // Perform checks or operations 
    return await next(); 
    // Continue to next middleware or complete navigation 
}

// Redirect to login 
public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next) 
{ 
    if (!_authService.IsAuthenticated) 
    { 
        return NavResult.Redirect("/login?returnUrl=" + Uri.EscapeDataString(ctx.Path)); 
    }
    return await next(); 
}

// Cancel navigation with unsaved changes 
public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next) 
{ 
    if (_editState.HasUnsavedChanges) 
    { 
        var confirmed = await _dialogService.ConfirmAsync("You have unsaved changes. Continue anyway?"); 
        if (!confirmed) 
        { 
            return NavResult.Cancel(); 
        } 
    } 
    return await next(); 
}

// Return navigation error 
public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next) 
{ 
    try 
    { 
        return await next(); 
    } 
    catch (Exception ex) 
    { 
        _logger.LogError(ex, "Navigation error"); 
        return NavResult.Error(ex); 
    } 
}
```

## Data Loaders

ModernRouter enables async data loading for routed components:
- Implement the `IRouteDataLoader` interface for your loader class.
- Attach your loader to a component using the `[RouteDataLoader(typeof(MyLoader))]` attribute.
- The loader's `LoadAsync` method runs after navigation guards and before rendering.
- The loader result is provided to the component and its descendants via `[CascadingParameter]`.
- Supports DI and cancellation tokens for efficient, robust data fetching.

### Using a Data Loader

```csharp
public class ProductLoader : IRouteDataLoader<ProductData> 
{ 
    private readonly IProductService _productService;
    public ProductLoader(IProductService productService)
    {
        _productService = productService;
    }
    
    public async Task<ProductData> LoadAsync(RouteContext context, CancellationToken cancellationToken)
    {
        string productId = context.Parameters["id"]?.ToString() ?? "";
        return await _productService.GetProductAsync(productId, cancellationToken);
    }
}
```

## Error Boundaries

ModernRouter includes a robust error boundary system that captures and displays exceptions from all parts of the routing pipeline without crashing your application.

### Global Error Handling

Define application-wide error handling by providing an `ErrorContent` template to the Router component:

```razor
<ModernRouter.Components.Router AppAssembly="@typeof(Program).Assembly"> 
    <NotFound> 
        <h1>Page not found</h1>
    </NotFound>
    <ErrorContent Context="exception"> 
        <div class="error-container"> 
            <h2>An error occurred</h2> 
            <p>@exception.Message</p> 
            <button @onclick="RetryNavigation">Retry</button> 
        </div> 
    </ErrorContent> 
</ModernRouter.Components.Router>
```

### Comprehensive Error Capture

The error boundary system catches exceptions from all phases of routing:

1. **Navigation Middleware**: When middleware returns `NavResult.Error()` or throws an exception
2. **Data Loaders**: When a route's data loader throws during `LoadAsync()`
3. **Component Rendering**: When a component throws during initialization or rendering

### Benefits

- **Circuit Preservation**: Errors are contained without tearing down the Blazor circuit
- **Contextual Handling**: Different parts of your app can handle errors differently
- **User Experience**: Provide helpful recovery options instead of blank screens
- **Nested Recovery**: Child routes can recover independently from parent routes
- **Consistency**: All error types are channeled through the same error templates

### Example: Error Recovery

```csharp
@page "/products/{id:int}" 
@attribute [RouteDataLoader(typeof(ProductLoader))] 
@inject ProductService ProductService
<h1>Product Details</h1>
@if (_loadError && _product == null) 
{ 
    <div class="alert alert-danger"> <p>Failed to load product. Please try again.</p> <button @onclick="RetryLoad" class="btn btn-primary">Retry</button> </div> 
} 
else if (_product != null) 
{ 
    <div class="product-details"> <h2>@_product.Name</h2> <p>@_product.Description</p> <p>Price: @_product.Price.ToString("C")</p> </div> 
}

@code { 
    [CascadingParameter] private ProductData? _product { get; set; } 
    [Parameter] public int Id { get; set; } 
    private bool _loadError;
    
    private async Task RetryLoad()
    {
        try {
            _loadError = false;
            await ProductService.RefreshProductAsync(Id);
            // Navigation will reload the component with fresh data
            await NavManager.RefreshCurrentAsync();
        }
        catch {
            _loadError = true;
        }
    }
}
```

### Per-View Error Boundaries

Each route and outlet can define its own error boundary for more contextual error handling:

```csharp
<div class="dashboard-panel"> 
    <h3>User Statistics</h3> 
    <Outlet Name="userStats"> 
        <ErrorContent Context="error"> 
            <div class="panel-error"> 
                <p>Failed to load user statistics: @error.Message</p> 
                <button @onclick="RefreshStats">Retry</button> 
            </div> 
        </ErrorContent> 
    </Outlet>
</div>
```

## Route Metadata and Authorization

ModernRouter extracts component attributes into route entries, allowing middleware to access metadata without runtime reflection.

### Applying Metadata to Routes

Add attributes to your component classes:

```csharp
[Authorize]
[RouteDataLoader(typeof(ProductLoader))] 
public class ProductDetail : ComponentBase 
{ // Component implementation }

```

### Built-in Authorization Support

ModernRouter includes ASP.NET Core-compatible authorization attributes:

- **[Authorize]**: Requires authenticated user
- **[Authorize(Roles = "Admin,Manager")]**: Requires specific roles
- **[Authorize(Policy = "CanEditContent")]**: Requires policy-based authorization
- **[AllowAnonymous]**: Overrides authorization requirements

### Creating Custom Metadata Attributes

You can create and consume your own route metadata attributes:
```csharp
// Define attribute 
[AttributeUsage(AttributeTargets.Class)] public class FeatureFlagAttribute : Attribute 
{ 
    public string FeatureName { get; } 
    public FeatureFlagAttribute(string featureName) => FeatureName = featureName; 
}
// Apply to component 
[FeatureFlag("BetaFeature")] 
[Route("/beta-feature")] 
public class BetaComponent : ComponentBase { }

// Use in middleware 
public class FeatureFlagMiddleware : INavMiddleware 
{ 
    private readonly IFeatureService _featureService;
    public FeatureFlagMiddleware(IFeatureService featureService)
    {
        _featureService = featureService;
    }
    
    public Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next)
    {
        if (ctx.Match?.Matched is null)
            return next();
            
        var flagAttr = ctx.Match.Matched.Attributes
                        .OfType<FeatureFlagAttribute>()
                        .FirstOrDefault();
                        
        if (flagAttr != null && !_featureService.IsEnabled(flagAttr.FeatureName))
            return Task.FromResult(NavResult.Redirect("/feature-disabled"));
            
        return next();
    }
}

```

### Registering Authorization

```csharp
// Program.cs 
builder.Services.AddSingleton<IAuthorizationService, YourAuthService>(); 
builder.Services.AddScoped<INavMiddleware, AuthorizationMiddleware>();

```

## Implementing Authorization Service

```csharp
public class YourAuthService : IAuthorizationService 
{ 
    private readonly AuthenticationStateProvider _authStateProvider;
    public YourAuthService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }
    
    public bool IsAuthenticated()
    {
        var authState = _authStateProvider.GetAuthenticationStateAsync().Result;
        return authState.User.Identity?.IsAuthenticated ?? false;
    }
    
    public bool IsInRoles(IEnumerable<string> roles)
    {
        var authState = _authStateProvider.GetAuthenticationStateAsync().Result;
        return roles.Any(role => authState.User.IsInRole(role));
    }
    
    public async Task<bool> AuthorizeAsync(string policy)
    {
        // Your policy-based authorization logic here
        return await Task.FromResult(true);
    }
}

```

## Technical Requirements

- .NET 9
- C# 13.0
- Blazor WebAssembly

## Contributing

Contributions are welcome! Please read the [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines.

## License

ModernRouter is licensed under the [MIT License](LICENSE.md).

## Support

For support, please open an issue on the [GitHub repository](https://github.com/vangundy/ModernRouter/issues).

##TODO

- [ ] Add more examples and documentation for advanced scenarios
- [ ] Implement breadcrumb component that renders based on router state 
- [ ] Add support for global progress indicators during navigation.  Approach: Pass a <LoadingContent> fragment to the Router component that will be displayed during navigation. It flows to all nested outlets.
- [ ] Add support for lazy loading of route components
- [ ] Implement route prefetching for improved performance
- [ ] Add support for scroll restoration on navigation
- [ ] Implement route transition animations
- [ ] Add support for query string parameters in routes
- [ ] Implement route guards for specific conditions
- [ ] Add support for source generation of route tables to improve performance. Approach: Use source generators to analyze route attributes and generate a strongly-typed route table (route manifest) at compile time using Roslyn Source Generator.
- [ ] Concurrent prefetching of data loaders. Approach: Schedule LoadAsync for all data loaders in parallel, then wait for all to complete before rendering the component. Requires a manifest walk.
- [ ] Add support for route aliases
- [ ] Implement caching for loaders to enhance performance and reduce redundant data fetching.
- [ ] Build-time type safety for loaders. Approach: Use source generators to validate loader signatures at compile time.
- [ ] Per navigation DI scope for loaders and middleware. Approach: Create a scoped service provider for each navigation that can be injected into loaders and middleware.
- [ ] Support Blazor server-side scenarios. 
- [ ] Link prefetch eliminates first-click delay by prefetching route data when hovering over links.
- [ ] Optimistic UI skeleton loading for data-heavy routes.
- [ ] Nav error toast notifications for better user feedback.
- [ ] Route aware devtools for debugging and inspection.
- [ ] Prefetch header hints for improved performance leveraging HTTP/2 multiplexing.