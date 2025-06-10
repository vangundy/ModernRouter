# ModernRouter Tests - Edge Cases

This test suite focuses on comprehensive edge case testing for the ModernRouter library, covering security vulnerabilities, performance issues, and error conditions.

## Test Coverage

### High Priority Edge Cases ✅

1. **RouteMatcher Edge Cases** (`RouteMatcherEdgeCaseTests.cs`)
   - Malformed URLs and invalid characters
   - Path traversal attempts (e.g., `../../../etc/passwd`)
   - XSS injection attempts
   - Extremely long paths and parameters
   - Invalid parameter types (e.g., strings for int constraints)
   - URL encoding/decoding edge cases
   - Route alias matching scenarios

2. **URL Validation and Security** (`UrlValidatorSecurityTests.cs`)
   - XSS attack prevention (`<script>`, `javascript:`, etc.)
   - Directory traversal protection
   - SQL injection attempt handling
   - Null byte injection
   - Invalid path characters
   - Path length limits
   - Query string validation

3. **Query Parameter Parsing** (`QueryParametersEdgeCaseTests.cs`)
   - Malformed query strings
   - URL encoding/decoding
   - Duplicate parameters
   - Empty values and keys
   - Special characters and Unicode
   - Extremely long parameters
   - Case sensitivity handling

4. **Breadcrumb Service Edge Cases** (`BreadcrumbServiceEdgeCaseTests.cs`)
   - Circular route references
   - Missing route metadata
   - Route hierarchy building with edge cases
   - Parameter substitution failures
   - Large route collections performance

### Medium Priority Edge Cases ✅

5. **Named Route Service** (`RouteNameServiceEdgeCaseTests.cs`)
   - Duplicate route names
   - Invalid parameter types
   - Thread safety testing
   - Large route collections
   - Parameter validation edge cases

6. **Route Animation Edge Cases** (`RouteAnimationServiceEdgeCaseTests.cs`)
   - Rapid navigation cancellation
   - JavaScript runtime disconnection
   - Animation timeout handling
   - Concurrent animation management
   - Malformed animation keyframes

7. **Middleware Pipeline** (`MiddlewarePipelineEdgeCaseTests.cs`)
   - Exception propagation
   - Circular dependency detection
   - Authorization edge cases
   - Cancellation token handling
   - Large middleware chains

## Test Structure

- **TestHelpers.cs**: Common utilities for creating test data
- Each test class focuses on a specific component
- Tests use FluentAssertions for readable assertions
- Moq for mocking dependencies
- Comprehensive edge case coverage including security scenarios

## Security Focus

The tests specifically address common web security vulnerabilities:
- **XSS Prevention**: Tests for script injection, HTML injection
- **Path Traversal**: Tests for `../` and `..\\` patterns
- **Parameter Injection**: Tests for malicious parameter values
- **Input Validation**: Tests for oversized inputs and invalid formats
- **URL Encoding**: Tests for encoded malicious content

## Performance Testing

Several tests verify performance characteristics:
- Large route collections (1000+ routes)
- Concurrent operations
- Memory usage patterns
- Timeout handling

## Known Test Issues

Some tests may fail due to:
1. Mocking limitations with extension methods (JSRuntime)
2. Internal class access restrictions
3. Missing test data setup for complex scenarios

## Usage

Run tests with:
```bash
dotnet test ModernRouter.Tests.csproj
```

For specific test categories:
```bash
dotnet test --filter Category=Security
dotnet test --filter Category=Performance  
```