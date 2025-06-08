using System;

namespace ModernRouter.Routing;

/// <summary>
/// Specifies an alias route template that can be used to access the same component.
/// Multiple aliases can be applied to a single component.
/// </summary>
/// <remarks>
/// Route aliases provide alternative URL paths to access the same component without duplicating @page directives.
/// This is useful for backward compatibility, SEO optimization, and providing user-friendly URL variations.
/// </remarks>
/// <example>
/// <code>
/// @page "/users/{id:int}"
/// @attribute [RouteAlias("/profile/{id:int}")]
/// @attribute [RouteAlias("/member/{id:int}")]
/// public partial class UserProfile : ComponentBase
/// {
///     [Parameter] public int Id { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RouteAliasAttribute : Attribute
{
    /// <summary>
    /// Gets the route template for this alias.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Gets or sets whether accessing this alias should redirect to the primary route.
    /// When true, the router will issue a redirect to the canonical URL.
    /// Default is false (no redirect).
    /// </summary>
    public bool RedirectToPrimary { get; set; } = false;

    /// <summary>
    /// Gets or sets the priority of this alias when multiple aliases could match.
    /// Higher numbers have higher priority. Default is 0.
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the RouteAliasAttribute class.
    /// </summary>
    /// <param name="template">The route template for this alias</param>
    /// <exception cref="ArgumentException">Thrown when template is null or empty</exception>
    public RouteAliasAttribute(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Route alias template cannot be null or empty.", nameof(template));
        
        Template = template;
    }
}