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
- **Query Parameter Support**: Full parsing utilities for URL query strings

### Named Route System
- **Type-Safe Navigation**: Navigate using route names instead of hardcoded URLs
- **URL Generation**: Generate URLs programmatically from route names and parameters
- **Route Registration**: Automatic registration of named routes from component attributes
- **Parameter Validation**: Validate parameters during URL generation

### Security & Validation
- **URL Validation**: Comprehensive validation against malicious patterns
- **XSS Protection**: Detection and prevention of cross-site scripting attempts
- **SQL Injection Protection**: Pattern detection for SQL injection attempts
- **Path Traversal Protection**: Prevents directory traversal attacks
- **Parameter Sanitization**: Automatic cleaning of dangerous characters

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

### Navigation Experience
- **Progress Indicators**: Built-in navigation progress indication
- **Loading States**: Configurable loading UI during route transitions
- **Error Boundaries**: Comprehensive error handling with recovery options

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

### Route Aliases
- **Multiple URL Paths**: Define alternative URLs that map to the same component
- **Backward Compatibility**: Maintain old URLs while migrating to new structures
- **Redirect Support**: Automatically redirect aliases to canonical URLs
- **Priority System**: Control which alias takes precedence in matching

### Enhanced Breadcrumbs
- **Intelligent Matching**: Smart breadcrumb generation based on route structure
- **Customizable Templates**: Flexible item and separator templates
- **Parameter-Aware**: Automatic handling of route parameters in breadcrumb labels
- **Accessibility**: Built-in ARIA attributes and semantic markup

## Installation

To install ModernRouter, add the library to your Blazor WebAssembly project using NuGet:

`dotnet add package ModernRouter`

## Quick Start

1. Add the ModernRouter library to your Blazor WebAssembly project.
2. Register ModernRouter services in your `Program.cs`:
```csharp
// Basic setup
builder.Services.AddModernRouter();

// With authorization support 
builder.Services.AddModernRouter(options => 
{ 
    options.EnableAuthorization = true;
    options.Authorization.LoginPath = "/auth/login"; 
    options.Authorization.ForbiddenPath = "/unauthorized"; 
});

// With animations
builder.Services.AddModernRouter(options =>
{
    options.Animations.EnableAnimations = true;
    options.Animations.DefaultDuration = 300;
    options.Animations.RespectReducedMotion = true;
});

// With both authorization and animations
builder.Services.AddModernRouter(options =>
{
    // Authorization
    options.EnableAuthorization = true;
    options.Authorization.LoginPath = "/login";
    options.Authorization.ForbiddenPath = "/forbidden";
    
    // Animations
    options.Animations.EnableAnimations = true;
    options.Animations.DefaultDuration = 500;
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
    <NavigationProgress>
        <div class="loading-spinner">
            <p>Loading...</p>
        </div>
    </NavigationProgress>
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
@page "/products" 
@page "/products/{Category}"
<h1>Products @(Category ?? "All Categories")</h1>
@code { 
    [Parameter] public string? Category { get; set; } 
}
```

### Named Routes

```razor
@page "/users/{id:int}"
@attribute [RouteName("UserProfile")]
<h1>User Profile: @Id</h1>
@code { 
    [Parameter] public int Id { get; set; } 
}
```

```csharp
@inject NavigationManager Nav
@inject IRouteNameService RouteNames

// Navigate using route names
Nav.NavigateToNamedRoute(RouteNames, "UserProfile", new { id = 123 });

// Generate URLs
string url = Nav.GetUrlForNamedRoute(RouteNames, "UserProfile", new { id = 123 });
```

#### **Named Routes with Aliases**

Named routes always generate primary URLs, ensuring consistency:

```razor
@page "/users/{id:int}"
@attribute [RouteAlias("/profile/{id:int}")]
@attribute [RouteAlias("/u/{id:int}")]
@attribute [RouteName("UserProfile")]
```

```csharp
// This always generates "/users/123", never aliases
string canonicalUrl = Nav.GetUrlForNamedRoute(RouteNames, "UserProfile", new { id = 123 });
// Result: "/users/123" (primary route)

// But users can still access via aliases:
// - /profile/123 (works)
// - /u/123 (works)
// - All lead to the same component
```

**Benefits:**
- **Consistent URL generation**: Always produces canonical URLs
- **SEO-friendly**: Prevents duplicate content issues
- **Predictable behavior**: Named routes unaffected by aliases
- **Analytics tracking**: Easy to distinguish primary vs alias usage

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

### Route Transition Animations

ModernRouter provides smooth, performant route transition animations using CSS-based effects that respect user accessibility preferences.

#### **Basic Usage**

```razor
@page "/my-page"
@using ModernRouter.Animations
@attribute [RouteAnimation("fadeIn", "fadeOut")]

<h1>My Animated Page</h1>
```

#### **Built-in Animations**

- **Fade**: `fadeIn`, `fadeOut` - Smooth opacity transitions
- **Slide**: `slideLeft`, `slideRight`, `slideUp`, `slideDown` - Directional slide effects
- **Scale**: `scaleIn`, `scaleOut` - Zoom-like scaling transitions
- **Zoom**: `zoomIn`, `zoomOut` - Dramatic zoom effects
- **Page Transition**: `pageTransition` - Subtle slide perfect for general navigation

#### **Custom Configuration**

```razor
@attribute [RouteAnimation(
    EnterAnimation = "scaleIn", 
    ExitAnimation = "fadeOut",
    Duration = 500,
    Easing = AnimationEasing.EaseOutBack
)]
```

#### **Service Registration**

```csharp
// Basic animations (enabled by default)
builder.Services.AddModernRouter();

// With animation configuration
builder.Services.AddModernRouter(options =>
{
    options.Animations.EnableAnimations = true;
    options.Animations.DefaultDuration = 300;
    options.Animations.RespectReducedMotion = true;
});
```

#### **Animation Lifecycle Hooks**

Components can implement `IAnimationLifecycleHooks` for custom animation logic:

```csharp
public class MyComponent : ComponentBase, IAnimationLifecycleHooks
{
    public async Task OnAnimationStartAsync(AnimationPhase phase, RouteAnimationContext context)
    {
        // Custom logic when animation starts
    }

    public async Task OnAnimationCompleteAsync(AnimationPhase phase, RouteAnimationContext context, AnimationResult result)
    {
        // Custom logic when animation completes
    }

    public async Task OnAnimationCancelledAsync(AnimationPhase phase, RouteAnimationContext context)
    {
        // Custom logic when animation is cancelled
    }
}
```

#### **Performance & Accessibility**

- **GPU Accelerated**: CSS-based animations leverage hardware acceleration
- **Cancellation Support**: Animations respect navigation cancellation tokens
- **Reduced Motion**: Automatically respects `prefers-reduced-motion` media query
- **Minimal Overhead**: Only adds cost when animations are configured

### Route Aliases

Route aliases allow you to define multiple URL paths that lead to the same component, enabling powerful scenarios for real-world applications.

#### **Basic Usage**

```razor
@page "/users/{id:int}"
@attribute [RouteAlias("/profile/{id:int}")]
@attribute [RouteAlias("/member/{id:int}")]
@attribute [RouteAlias("/u/{id:int}", Priority = 10)]
@attribute [RouteName("UserProfile")]

<h1>User Profile: @Id</h1>

@code {
    [Parameter] public int Id { get; set; }
}
```

#### **Redirect-to-Primary Aliases**

```razor
@page "/articles/{id:int}"
@attribute [RouteAlias("/posts/{id:int}", RedirectToPrimary = true)]
@attribute [RouteAlias("/blog/{id:int}", RedirectToPrimary = true)]

<h1>Article @Id</h1>
<!-- Legacy URLs /posts/{id} and /blog/{id} redirect to /articles/{id} -->

@code {
    [Parameter] public int Id { get; set; }
}
```

## Route Aliases - Real-World Use Cases

### **1. URL Migration & Legacy Support**

When restructuring your application's URLs, route aliases provide seamless backward compatibility:

```razor
// Scenario: Migrating from /admin/users to /dashboard/users
@page "/dashboard/users/{id:int}"
@attribute [RouteAlias("/admin/users/{id:int}", RedirectToPrimary = true)]
@attribute [RouteName("UserManagement")]

<h1>User Management</h1>

@code {
    [Parameter] public int Id { get; set; }
}
```

**Benefits:**
- Old bookmarks continue working
- External links remain valid
- SEO ranking preserved through redirects
- Gradual migration without breaking changes

### **2. SEO Optimization & Marketing**

Create multiple SEO-friendly URLs for the same content:

```razor
@page "/services/web-development"
@attribute [RouteAlias("/web-design")]
@attribute [RouteAlias("/website-creation")]
@attribute [RouteAlias("/custom-web-solutions")]

<h1>Web Development Services</h1>
```

**Use Cases:**
- **Keyword targeting**: Different URLs target different search terms
- **Campaign URLs**: Marketing campaigns can use branded short URLs
- **A/B testing**: Test which URL structure performs better
- **Regional variations**: Different terms for different markets

### **3. User-Friendly Shortcuts**

Provide convenient shortcuts alongside formal URLs:

```razor
@page "/user/settings/account/profile"
@attribute [RouteAlias("/profile")]
@attribute [RouteAlias("/me")]
@attribute [RouteAlias("/account")]

<h1>User Profile Settings</h1>
```

**Benefits:**
- Easier to remember and type
- Better user experience
- Reduced cognitive load
- Faster navigation for power users

### **4. Internationalization Support**

Support multiple languages in URLs:

```razor
@page "/en/about-us"
@attribute [RouteAlias("/about")]              <!-- Default English -->
@attribute [RouteAlias("/es/acerca-de")]       <!-- Spanish -->
@attribute [RouteAlias("/fr/a-propos")]        <!-- French -->
@attribute [RouteAlias("/de/uber-uns")]        <!-- German -->

<h1>About Us</h1>
```

### **5. API Versioning**

Maintain multiple API versions while defaulting to the latest:

```razor
@page "/api/v3/products/{id:int}"
@attribute [RouteAlias("/api/products/{id:int}")]        <!-- Default to latest -->
@attribute [RouteAlias("/api/v2/products/{id:int}", RedirectToPrimary = true)]  <!-- Upgrade v2 -->

<h1>Product API</h1>

@code {
    [Parameter] public int Id { get; set; }
}
```

## Priority System - Solving Route Conflicts

The priority system solves the problem of conflicting route patterns where multiple aliases could match the same URL.

### **The Problem**

Consider this scenario without priorities:

```razor
@page "/products/{category}"
@attribute [RouteAlias("/p/{category}")]
@attribute [RouteAlias("/shop/{category}")]

// Another component
@page "/products/{id:int}"
@attribute [RouteAlias("/p/{id:int}")]
```

**Issue**: The URL `/p/123` could match either:
- `/p/{category}` (treating "123" as a category string)
- `/p/{id:int}` (treating "123" as an integer ID)

### **The Solution: Priority System**

```razor
// Product by category
@page "/products/{category}"
@attribute [RouteAlias("/p/{category}", Priority = 1)]
@attribute [RouteAlias("/shop/{category}")]

// Product by ID (higher priority)
@page "/products/{id:int}"
@attribute [RouteAlias("/p/{id:int}", Priority = 10)]
```

**Result**: `/p/123` will match the higher priority alias (`Priority = 10`) first, ensuring it's treated as an integer ID rather than a category string.

### **Priority Best Practices**

1. **Higher numbers = Higher priority** (Priority 10 beats Priority 1)
2. **Default priority is 0** if not specified
3. **Use sparingly** - only when you have genuine conflicts
4. **Document priorities** - make it clear why certain aliases have priority

### **Complex Priority Example**

```razor
// E-commerce product pages with conflicting patterns
@page "/products/{productId:int}"
@attribute [RouteAlias("/p/{productId:int}", Priority = 20)]      <!-- Highest: specific ID -->
@attribute [RouteAlias("/item/{productId:int}", Priority = 15)]

@page "/products/{category}"
@attribute [RouteAlias("/p/{category}", Priority = 10)]          <!-- Medium: category -->
@attribute [RouteAlias("/cat/{category}", Priority = 5)]

@page "/products/special-offers"
@attribute [RouteAlias("/p/deals", Priority = 25)]               <!-- Highest: exact match -->
@attribute [RouteAlias("/deals")]
```

**Matching Order for `/p/deals`:**
1. Priority 25: `/p/deals` (exact match) ✅ **WINS**
2. Priority 20: `/p/{productId:int}` (would fail - "deals" isn't an int)
3. Priority 10: `/p/{category}` (would match as category)

**Matching Order for `/p/123`:**
1. Priority 25: `/p/deals` (doesn't match)
2. Priority 20: `/p/{productId:int}` ✅ **WINS** (123 is valid int)
3. Priority 10: `/p/{category}` (would also match but lower priority)

## Advanced Route Alias Features

### **Detecting Alias Usage**

Access alias information in your components:

```razor
@code {
    [CascadingParameter] private RouteContext? RouteContext { get; set; }

    protected override void OnInitialized()
    {
        if (RouteContext?.IsAliasMatch == true)
        {
            var aliasUsed = RouteContext.MatchedTemplate;
            var primaryRoute = RouteContext.Matched?.TemplateString;
            
            // Log analytics: which alias brought user to this page
            Analytics.Track("AliasUsed", new { 
                Alias = aliasUsed, 
                Canonical = primaryRoute 
            });
        }
    }
}
```

### **Conditional Redirects**

Create smart redirects based on business logic:

```razor
@page "/shop/{category}"
@attribute [RouteAlias("/store/{category}", RedirectToPrimary = true)]   <!-- Always redirect -->
@attribute [RouteAlias("/mall/{category}")]                             <!-- No redirect -->

<h1>Shopping: @Category</h1>

@code {
    [Parameter] public string Category { get; set; } = "";
    [CascadingParameter] private RouteContext? RouteContext { get; set; }

    protected override void OnInitialized()
    {
        // Custom redirect logic for specific aliases
        if (RouteContext?.IsAliasMatch == true && 
            RouteContext.MatchedTemplate.Contains("/mall/") && 
            IsBlackFridaySeason())
        {
            // Redirect mall alias to special Black Friday page during November
            NavigationManager.NavigateTo($"/black-friday/{Category}");
        }
    }
}
```

### **Error Handling & Fallbacks**

Route aliases integrate with ModernRouter's error handling:

```razor
@page "/docs/{section}"
@attribute [RouteAlias("/help/{section}")]
@attribute [RouteAlias("/support/{section}")]

@code {
    protected override void OnParametersSet()
    {
        // Validate section parameter regardless of which alias was used
        if (!IsValidSection(Section))
        {
            // ModernRouter error handling will catch this
            throw new ArgumentException($"Invalid documentation section: {Section}");
        }
    }
}
```

## Route Alias Performance & Best Practices

### **Performance Optimization**

ModernRouter's route alias implementation is designed for optimal performance:

1. **Primary Route First**: Always tries the primary route before checking aliases
2. **Short-Circuit Matching**: Stops at first successful match
3. **Priority Sorting**: Higher priority aliases checked first within each route
4. **Template Caching**: Parsed route templates are cached during build time
5. **Minimal Overhead**: Aliases only add cost when primary route fails

### **Best Practices**

#### **Use Aliases Strategically**
```razor
// ✅ Good: Meaningful aliases for different user types
@page "/dashboard/users/{id:int}"
@attribute [RouteAlias("/admin/users/{id:int}")]        <!-- Legacy admin URL -->
@attribute [RouteAlias("/manage/users/{id:int}")]       <!-- Manager-friendly URL -->

// ❌ Avoid: Too many similar aliases
@page "/users/{id:int}"
@attribute [RouteAlias("/user/{id:int}")]
@attribute [RouteAlias("/usr/{id:int}")]
@attribute [RouteAlias("/u/{id:int}")]
@attribute [RouteAlias("/person/{id:int}")]              <!-- Excessive -->
```

#### **Minimize Priority Usage**
```razor
// ✅ Good: Use priorities only when necessary
@page "/products/{id:int}"
@attribute [RouteAlias("/p/{id:int}", Priority = 10)]   <!-- Only when conflicts exist -->

// ❌ Avoid: Unnecessary priorities
@page "/about"
@attribute [RouteAlias("/about-us", Priority = 5)]      <!-- No conflict, priority not needed -->
```

#### **Document Alias Purposes**
```razor
// ✅ Good: Clear documentation
@page "/articles/{id:int}"
@attribute [RouteAlias("/posts/{id:int}", RedirectToPrimary = true)]  <!-- Legacy CMS migration -->
@attribute [RouteAlias("/blog/{id:int}", RedirectToPrimary = true)]   <!-- Marketing campaign URLs -->
```

### **Migration Strategy**

When implementing route aliases in existing applications:

1. **Phase 1**: Add aliases without redirects to test compatibility
2. **Phase 2**: Enable redirects for critical SEO URLs
3. **Phase 3**: Monitor analytics to identify unused aliases
4. **Phase 4**: Remove obsolete aliases after sufficient migration time

The route aliases system provides enterprise-grade URL management capabilities while maintaining ModernRouter's focus on security, performance, and developer experience.

### Enhanced Breadcrumbs

```razor
@page "/products/{category}/{id:int}"
@using ModernRouter.Components

<Breadcrumbs />
<h1>Product Details</h1>

@code {
    [Parameter] public string Category { get; set; } = string.Empty;
    [Parameter] public int Id { get; set; }
}
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

1. **Router**: The main router component that initiates the routing process and manages navigation.
2. **RouteTableFactory**: Builds a routing table by analyzing component route attributes and processing aliases.
3. **RouteMatcher**: Matches URL paths against route templates and aliases with security validation.
4. **Outlet**: Renders matched components at specific locations in the component hierarchy.
5. **RouteView**: Renders individual route components with loading and error handling.
6. **RouteContext**: Encapsulates routing state, parameters, and alias information.
7. **INavMiddleware**: Interface for navigation middleware guards.
8. **IRouteDataLoader**: Interface for async data loaders.
9. **IRouteTableService**: Centralized route management and breadcrumb generation.
10. **IRouteNameService**: Named route registration and URL generation.
11. **Breadcrumbs**: Intelligent breadcrumb component with customizable templates.
12. **RouteAliasAttribute**: Defines alternative URL paths with redirect and priority options.

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

## Roadmap

### Completed Features ✅
- [x] Enhanced breadcrumb component with intelligent route matching
- [x] Global progress indicators during navigation
- [x] Query string parameter support in routes
- [x] Named route system with type-safe navigation
- [x] Comprehensive URL validation and security features
- [x] Route table service for centralized route management
- [x] Route aliases with redirect support and priority system
- [x] **Route transition animations** - CSS-based animations with lifecycle hooks

### Planned Features 🚧
- [ ] **Route caching system** - LRU cache for route match results in high-traffic scenarios
- [ ] Add support for lazy loading of route components
- [ ] Implement route prefetching for improved performance
- [ ] Add support for scroll restoration on navigation
- [ ] Add support for source generation of route tables to improve performance
- [ ] Concurrent prefetching of data loaders
- [ ] Implement caching for loaders to enhance performance and reduce redundant data fetching
- [ ] Build-time type safety for loaders using source generators
- [ ] Per navigation DI scope for loaders and middleware
- [ ] Support Blazor server-side scenarios
- [ ] Link prefetch to eliminate first-click delay
- [ ] Optimistic UI skeleton loading for data-heavy routes
- [ ] Navigation error toast notifications for better user feedback
- [ ] Route-aware devtools for debugging and inspection
- [ ] Prefetch header hints for improved performance leveraging HTTP/2 multiplexing