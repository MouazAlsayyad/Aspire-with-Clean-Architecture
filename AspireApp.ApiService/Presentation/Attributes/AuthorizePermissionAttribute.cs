using Microsoft.AspNetCore.Authorization;

namespace AspireApp.ApiService.Presentation.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public AuthorizePermissionAttribute(params string[] permissions)
    {
        Policy = $"Permission:{string.Join(",", permissions)}";
    }
}

