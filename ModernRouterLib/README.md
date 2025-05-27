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

## Architecture

ModernRouter uses a component-based architecture with these key parts:

1. Router: The main router component that initiates the routing process
2. RouteTableFactory: Builds a routing table by analyzing component route attributes
3. RouteMatcher: Matches URL paths against route templates
4. Outlet: Renders matched components at specific locations in the component hierarchy
5. RouteContext: Encapsulates routing state and parameters

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

## Example Usage Patterns

- Master-detail views with independent routing for each section
- Tabbed interfaces with URL-based navigation
- Wizard-like flows with URL history support
- Deep-linking to specific application states
