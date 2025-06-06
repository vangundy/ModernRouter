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