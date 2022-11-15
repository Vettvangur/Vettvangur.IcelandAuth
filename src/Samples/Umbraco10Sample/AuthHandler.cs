using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Vettvangur.IcelandAuth;

namespace Umbraco10Sample;

class AuthHandler
{
    readonly ILogger _logger;
    readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHandler(
        ILogger<AuthHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> HandleLogin(SamlLogin login)
    {
        var httpCtx = _httpContextAccessor.HttpContext;
        if (httpCtx == null)
        {
            return await HandleError(login); 
        }
        
        var ms = httpCtx.RequestServices.GetService<IMemberService>();
        var mm = httpCtx.RequestServices.GetService<IMemberManager>();
        var msim = httpCtx.RequestServices.GetService<IMemberSignInManager>();
        
        var member = ms.GetByUsername(login.UserSSN);

        if (member == null)
        {
            _logger.LogInformation($"Creating new User: {login.UserSSN}");

            member = ms.CreateMemberWithIdentity(
                login.UserSSN,
                login.UserSSN + "@example.com",
                login.Name,
                "Member"
            );

            ms.AssignRole(member.Id, "Members");
            ms.Save(member);
        }

        var m = await mm.FindByIdAsync(member.Key.ToString());
        await msim.SignInAsync(m, true);
        // This causes all subsequent requests for the user to be 
        // authenticated as the given umbraco member

        // Provide a way for views and services to access the sessions saml login result
        //HttpContext.Session["samlLogin"] = login;

        // Return a custom redirect url
        return null;
    }

    public Task<string> HandleError(SamlLogin login)
    {
        _logger.LogError("Error encountered while attempting Ísland.is authentication.");

        // Handle erronous logins here
        if (login != null)
        {

        }

        return Task.FromResult("/?error=UnableToLogin");
    }
}
