# ModernRouter

ModernRouter is a powerful and flexible routing library for Blazor WebAssembly applications built with .NET 9 and C# 13.0. It provides an advanced, hierarchical routing system designed to handle complex routing scenarios in modern web applications.

## Key Features

### Advanced Route Parameter Handling
- Type-Constrained Parameters: Specify data types for route parameters (int, guid, datetime, etc.)
- Optional Parameters: Define parameters that don't need to be included in the URL
- Catch-All Parameters: Capture multiple URL segments with a single parameter
- Strong Typing: Route parameters are automatically converted to their specified types

### Hierarchical Routing with Outlets
- Nested Routes: Support for parent-child relationships between routes
- Component Outlets: Place child components at specific locations within parent components
- Context Preservation: Route data and parameters are cascaded through the component hierarchy
- Remaining Segment Handling: Unmatched segments are passed to child outlets

### Intelligent Route Matching
- Priority-Based Matching: Routes are matched in order of specificity
- Type Validation: Parameter constraints are enforced during matching
- Case-Insensitive Matching: Route segments match regardless of case
- Flexible Template Structure: Support for complex route patterns

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

## Architecture

ModernRouter uses a component-based architecture with these key parts:

1. Router: The main router component that initiates the routing process
2. RouteTableFactory: Builds a routing table by analyzing component route attributes
3. RouteMatcher: Matches URL paths against route templates
4. Outlet: Renders matched components at specific locations in the component hierarchy
5. RouteContext: Encapsulates routing state and parameters
6. INavMiddleware: Interface for navigation middleware guards
7. IRouteDataLoader: Interface for async data loaders

## Middleware Guards

ModernRouter supports a middleware pipeline for navigation, allowing you to add cross-cutting concerns to routing. Middleware guards implement the `INavMiddleware` interface and can:
- Inspect or modify navigation context
- Allow, redirect, or cancel navigation
- Be chained in a configurable order

**Common Middleware Examples:**
- **AuthGuard**: Restricts access to certain routes based on authentication state
- **UnsavedGuard**: Prompts users about unsaved changes before navigating away
- **AnalyticsTap**: Tracks page views or navigation events for analytics

To register middleware guards, add them to the DI container in your `Program.cs`:

    builder.Services.AddScoped<INavMiddleware, AnalyticsTap>();
    builder.Services.AddScoped<INavMiddleware, AuthGuard>();
    builder.Services.AddScoped<INavMiddleware, UnsavedGuard>();

## Data Loaders

ModernRouter enables async data loading for routed components:
- Implement the `IRouteDataLoader` interface for your loader class
- Attach your loader to a component using the `[RouteDataLoader(typeof(MyLoader))]` attribute
- The loader's `LoadAsync` method runs after navigation guards and before rendering
- The loader result is provided to the component and its descendants via `[CascadingParameter]`
- Supports DI and cancellation tokens for efficient, robust data fetching

## Technical Requirements

- .NET 9
- C# 13.0
- Blazor WebAssembly

## Getting Started

1. Add the ModernRouter library to your Blazor WebAssembly project
2. Set up your main router in App.razor
3. Define routes using page directives on your components
4. Place Outlet components where nested content should appear
5. Use strongly-typed parameters in your components
6. Register any navigation middleware guards as needed
7. Implement and attach data loaders as needed

## Example Usage Patterns

- Master-detail views with independent routing for each section
- Tabbed interfaces with URL-based navigation
- Wizard-like flows with URL history support
- Deep-linking to specific application states
- Async data fetching for each route and nested outlet
