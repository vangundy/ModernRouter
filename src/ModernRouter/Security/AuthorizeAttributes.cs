namespace ModernRouter.Security;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    public string? Policy { get; set; }
    public string? Roles { get; set; }
    
    public AuthorizeAttribute() { }
    
    public AuthorizeAttribute(string policy)
    {
        Policy = policy;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AllowAnonymousAttribute : Attribute
{
}