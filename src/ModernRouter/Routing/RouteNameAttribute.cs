namespace ModernRouter.Routing;

/// <summary>
/// Attribute to specify a name for a route that can be used for URL generation
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RouteNameAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the route
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the RouteNameAttribute
    /// </summary>
    /// <param name="name">The name to assign to the route</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
    public RouteNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Route name cannot be null or empty", nameof(name));
            
        Name = name;
    }
}