using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PartsUnlimited.Controllers;

/// <summary>
/// Minimal AccountController used only when Entra ID is NOT configured in Development.
/// When Entra ID IS configured, sign-in/out routes are handled by Microsoft.Identity.Web.UI
/// Razor pages (/MicrosoftIdentity/Account/SignIn, SignOut).
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    public IActionResult AccessDenied() => View();
}
