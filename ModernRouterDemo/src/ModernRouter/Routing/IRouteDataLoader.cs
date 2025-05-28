using System.Threading;
using System.Threading.Tasks;

namespace ModernRouter.Routing;

public interface IRouteDataLoader
{
    Task<object?> LoadAsync(RouteContext context, IServiceProvider services, CancellationToken cancellationToken);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RouteDataLoaderAttribute : Attribute
{
    public Type LoaderType { get; }
    public RouteDataLoaderAttribute(Type loaderType) => LoaderType = loaderType;
}
