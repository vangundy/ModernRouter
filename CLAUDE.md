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

## Development Notes
- Target Framework: .NET 9.0
- Uses Blazor Razor components
- Nullable reference types enabled
- Implicit usings enabled
- Browser platform supported

## Common Tasks
- Adding new routing features
- Enhancing component functionality
- Security and authorization improvements
- Performance optimizations