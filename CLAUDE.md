# ModernRouter - Claude Code Context

## Project Overview
ModernRouter is a modern Blazor routing library built on .NET 9.0. It provides enhanced routing capabilities with features like breadcrumbs, outlets, route views, and authorization middleware.

## Project Structure
- `src/ModernRouter/` - Main library source code
  - `Components/` - Blazor components (Router, Outlet, RouteView, Breadcrumbs)
  - `Routing/` - Core routing logic and services
  - `Security/` - Authorization middleware and attributes
  - `Services/` - Route table factory and related services
  - `Extensions/` - Service collection extensions
- `samples/ModernRouterDemo/` - Demo application

## Key Components
- **Router.razor** - Main routing component
- **Outlet.razor** - Route outlet for nested routing
- **RouteView.razor** - Component for rendering route views
- **Breadcrumbs.razor** - Navigation breadcrumb component
- **RouteMatcher.cs** - Core route matching logic
- **AuthorizationMiddleware.cs** - Security and authorization

## Build Commands
```bash
# Build the solution
dotnet build ModernRouter.sln

# Build specific project
dotnet build src/ModernRouter/ModernRouter.csproj

# Run demo application
dotnet run --project samples/ModernRouterDemo/ModernRouterDemo.csproj

# Test (if tests exist)
dotnet test

# Pack NuGet package
dotnet pack src/ModernRouter/ModernRouter.csproj
```

## Service Registration
**REQUIRED**: Register ModernRouter services to use components:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddModernRouter();

// Or with authorization
builder.Services.AddModernRouterWithAuthorization(options =>
{
    options.LoginPath = "/login";
    options.ForbiddenPath = "/forbidden";
});

// Or with animations
builder.Services.AddModernRouterWithAnimations(options =>
{
    options.EnableAnimations = true;
    options.DefaultDuration = 300;
    options.RespectReducedMotion = true;
});
```

## Recent Enhancements
- ✅ Query parameter support with full parsing utilities
- ✅ Proper URL encoding/decoding for route parameters
- ✅ Intelligent breadcrumb route matching (replaces crude string detection)
- ✅ Route table service for centralized route management
- ✅ Comprehensive URL validation and sanitization
- ✅ Protection against XSS, SQL injection, and path traversal attacks
- ✅ Named route support for URL generation
- ✅ Type-safe navigation with route names
- ✅ Route aliases with redirect support and priority system
- ✅ Route transition animations with CSS-based effects and lifecycle hooks
- ✅ Service registration required for proper functionality

## Security Features
- **URL Validation**: Comprehensive validation against malicious patterns
- **Parameter Sanitization**: Automatic cleaning of dangerous characters
- **XSS Protection**: Detection and prevention of cross-site scripting attempts
- **SQL Injection Protection**: Pattern detection for SQL injection attempts
- **Path Traversal Protection**: Prevents directory traversal attacks
- **Malformed URL Protection**: Rejects invalid or suspicious URL formats

## Named Route Usage

### 1. Define Named Routes
```csharp
[Route("/users/{id:int}")]
[RouteName("UserProfile")]
public partial class UserProfile : ComponentBase
{
    [Parameter] public int Id { get; set; }
}

[Route("/users/{id:int}/posts/{slug}")]
[RouteName("UserPost")]
public partial class UserPost : ComponentBase
{
    [Parameter] public int Id { get; set; }
    [Parameter] public string Slug { get; set; }
}
```

### 2. Navigate Using Route Names
```csharp
@inject NavigationManager Nav
@inject IRouteNameService RouteNames

// Navigate with anonymous object
Nav.NavigateToNamedRoute(RouteNames, "UserProfile", new { id = 123 });

// Navigate with dictionary
Nav.NavigateToNamedRoute(RouteNames, "UserPost", new Dictionary<string, object?> 
{
    ["id"] = 123,
    ["slug"] = "my-blog-post"
});

// Generate URL without navigating
string url = Nav.GetUrlForNamedRoute(RouteNames, "UserProfile", new { id = 123 });
// Result: "/users/123"
```

### 3. Safe Navigation with Validation
```csharp
// Try navigation (returns false if route doesn't exist)
bool success = Nav.TryNavigateToNamedRoute(RouteNames, "UserProfile", new { id = 123 });

// Generate URL with validation
try 
{
    string url = RouteNames.GenerateUrl("UserProfile", new { id = 123 });
}
catch (ArgumentException ex)
{
    // Handle invalid route name or parameters
}
```

## Route Animation Usage

### Basic Animation Setup
```csharp
// Component with fade animation
@page "/my-page"
@using ModernRouter.Animations
@attribute [RouteAnimation("fadeIn", "fadeOut")]

<h1>My Animated Page</h1>
```

### Available Built-in Animations
- `fadeIn`, `fadeOut` - Smooth opacity transitions
- `slideLeft`, `slideRight`, `slideUp`, `slideDown` - Directional slides
- `scaleIn`, `scaleOut` - Zoom-like scaling effects
- `zoomIn`, `zoomOut` - Dramatic zoom transitions
- `pageTransition` - Subtle slide for general navigation
- `modalEntry` - Perfect for dialog-like pages

### Custom Animation Configuration
```csharp
@attribute [RouteAnimation(
    EnterAnimation = "scaleIn", 
    ExitAnimation = "fadeOut",
    Duration = 500,
    Easing = AnimationEasing.EaseOutBack
)]
```

### Animation Lifecycle Hooks
```csharp
public class MyComponent : ComponentBase, IAnimationLifecycleHooks
{
    public async Task OnAnimationStartAsync(AnimationPhase phase, RouteAnimationContext context)
    {
        // Custom logic when animation starts
        if (phase == AnimationPhase.Enter)
        {
            // Prepare for entry animation
        }
    }

    public async Task OnAnimationCompleteAsync(AnimationPhase phase, RouteAnimationContext context, AnimationResult result)
    {
        // Custom logic when animation completes
        if (result.Success && phase == AnimationPhase.Enter)
        {
            // Animation completed successfully
        }
    }

    public async Task OnAnimationCancelledAsync(AnimationPhase phase, RouteAnimationContext context)
    {
        // Handle animation cancellation
    }
}
```

## Development Notes
- Target Framework: .NET 9.0
- Uses Blazor Razor components
- Nullable reference types enabled
- Implicit usings enabled
- Browser platform supported

## Architecture Overview

### Core Components and Their Purpose

#### **Router** (`Router.razor` / `Router.razor.cs`)
**Purpose**: The main routing component that orchestrates the entire navigation system.

**Key Responsibilities**:
- Initializes the route table from assemblies using `RouteTableFactory`
- Listens to navigation events via `NavigationManager`
- Runs navigation through a middleware pipeline
- Manages loading states and error handling
- Handles route animations and cancellation tokens
- Provides cascading values to child components

#### **RouteView** (`RouteView.razor` / `RouteView.razor.cs`)
**Purpose**: Renders individual route components with data loading support.

**Key Responsibilities**:
- Uses `DynamicComponent` to render the matched route component
- Handles route data loading via `IRouteDataLoader`
- Manages loading states and error boundaries
- Passes route parameters and remaining segments to components

#### **Outlet** (`Outlet.razor` / `Outlet.razor.cs`)
**Purpose**: Enables nested routing by rendering child routes within parent layouts.

**Key Responsibilities**:
- Receives remaining path segments from parent routes
- Performs route matching on remaining segments
- Renders nested components with their own data loading

#### **Breadcrumbs** (`Breadcrumbs.razor` / `Breadcrumbs.razor.cs`)
**Purpose**: Automatically generates navigation breadcrumbs with hierarchical route analysis.

**Key Responsibilities**:
- Uses `IBreadcrumbService` to build breadcrumb hierarchy
- Supports both basic and hierarchical breadcrumb generation
- Provides customizable templates for rendering
- Includes route hierarchy information and debugging features
- Updates automatically on navigation changes

### Core Services

#### **IRouteTableService**
**Purpose**: Centralized management of route definitions and matching logic.

**Key Methods**:
- `Initialize(assemblies)` - Builds route table from assemblies
- `MatchRoute(path)` - Matches URL path to route definitions
- `GetBreadcrumbMatches(path)` - Builds breadcrumb hierarchy

#### **IRouteNameService**
**Purpose**: Handles named route URL generation for type-safe navigation.

#### **IBreadcrumbService**
**Purpose**: Builds breadcrumb hierarchies with support for both basic and hierarchical patterns.

#### **INavMiddleware Pipeline**
**Purpose**: Extensible middleware pipeline for intercepting and controlling navigation.

### Navigation Flow

Here's what happens when a user navigates to a page:

```
User clicks link/enters URL
         ↓
NavigationManager.LocationChanged event fires
         ↓
Router.NavigateAsync() called
         ↓
1. Cancel any pending navigation
2. Show progress indicator
3. Parse URL and extract path/query
4. RouteMatcher.Match() finds matching route
5. Create NavContext with route info
6. Run through middleware pipeline
         ↓
Middleware Pipeline (INavMiddleware[]):
- ErrorHandlingMiddleware (always first)
- AuthorizationMiddleware (if enabled)
- Custom middleware (AuthGuard, UnsavedGuard, etc.)
         ↓
Pipeline Results:
- NavResult.Allow() → Continue to render
- NavResult.Redirect(url) → Navigate to different URL
- NavResult.Cancel() → Restore previous URL
- NavResult.Error(ex) → Show error UI
         ↓
If Allow: Update Router._current = matchedRoute
         ↓
Router.razor template renders:
- RouteAnimationContainer (wraps with animations)
- RouteView component with matched route
         ↓
RouteView renders:
1. Check for IRouteDataLoader
2. If loader exists: show loading → call LoadAsync() → render component
3. If no loader: render component directly
4. Use DynamicComponent with route parameters
         ↓
Component renders with route parameters
```

### Component Hierarchy

```
┌─────────────────────────────────────────┐
│ App.razor                               │
│ ┌─────────────────────────────────────┐ │
│ │ Router                              │ │
│ │ • Manages navigation                │ │
│ │ • Runs middleware pipeline          │ │
│ │ • Handles errors & loading states   │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ RouteAnimationContainer         │ │ │
│ │ │ ┌─────────────────────────────┐ │ │ │
│ │ │ │ RouteView                   │ │ │ │
│ │ │ │ • Renders matched component │ │ │ │
│ │ │ │ • Handles data loading      │ │ │ │
│ │ │ │ • Error boundaries          │ │ │ │
│ │ │ │ ┌─────────────────────────┐ │ │ │ │
│ │ │ │ │ DynamicComponent        │ │ │ │ │
│ │ │ │ │ (Your Page Component)   │ │ │ │ │
│ │ │ │ │ ┌─────────────────────┐ │ │ │ │ │
│ │ │ │ │ │ Outlet (optional)   │ │ │ │ │ │
│ │ │ │ │ │ • Nested routing    │ │ │ │ │ │
│ │ │ │ │ │ • Child components  │ │ │ │ │ │
│ │ │ │ │ └─────────────────────┘ │ │ │ │ │
│ │ │ │ └─────────────────────────┘ │ │ │ │
│ │ │ └─────────────────────────────┘ │ │ │
│ │ └─────────────────────────────────┘ │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘

Separate Components:
┌─────────────────────┐
│ Breadcrumbs         │
│ • Uses Router state │
│ • Auto-updates      │
│ • Customizable UI   │
└─────────────────────┘
```

### Key Abstractions

#### **RouteEntry** - Route Definition
```csharp
public sealed record RouteEntry(RouteSegment[] Template, Type Component)
{
    public Type? LoaderType { get; init; }        // Data loader class
    public string? Name { get; init; }            // Named route identifier  
    public string TemplateString { get; init; }   // "/users/{id:int}"
    public IReadOnlyList<RouteAlias> Aliases { get; init; } // Alternative URLs
    public IReadOnlyList<Attribute> Attributes { get; init; } // Component attributes
}
```

#### **RouteContext** - Match Result
```csharp
public sealed class RouteContext
{
    public RouteEntry? Matched { get; init; }           // Matched route definition
    public string[] RemainingSegments { get; init; }    // For nested routing
    public Dictionary<string, object?> RouteValues { get; init; } // Route parameters
    public QueryParameters QueryParameters { get; init; } // Query string
    public bool IsAliasMatch { get; init; }             // Matched via alias
    public RouteAlias? MatchedAlias { get; init; }      // Which alias matched
}
```

### Example Usage Patterns

#### **Basic Route Definition**
```csharp
@page "/users/{id:int}"
@attribute [RouteName("UserProfile")]
@attribute [Breadcrumb("User Profile")]

<h1>User: @Id</h1>

@code {
    [Parameter] public int Id { get; set; }
}
```

#### **Nested Routing with Outlet**
```csharp
@page "/dashboard"
<h1>Dashboard</h1>

<nav>
    <a href="/dashboard/overview">Overview</a>
    <a href="/dashboard/reports">Reports</a>
</nav>

<ModernRouter.Components.Outlet />
```

#### **Custom Middleware**
```csharp
public class AuthGuard : INavMiddleware
{
    public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next)
    {
        // Check if route requires authentication
        if (RequiresAuth(ctx.Match?.Matched) && !IsAuthenticated())
            return NavResult.Redirect("/login");
            
        return await next(); // Continue pipeline
    }
}
```

## Common Tasks
- Adding new routing features
- Enhancing component functionality
- Security and authorization improvements
- Performance optimizations