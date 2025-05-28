namespace ModernRouter.Security;

public interface IAuthorizationService
{
    bool IsAuthenticated();
    bool IsInRoles(IEnumerable<string> roles);
    Task<bool> AuthorizeAsync(string policy);
}