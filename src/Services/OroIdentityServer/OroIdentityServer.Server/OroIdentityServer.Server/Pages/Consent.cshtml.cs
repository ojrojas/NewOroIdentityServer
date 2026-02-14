using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OroCQRS.Core.Interfaces;
using OroIdentityServer.Server.Services;

namespace OroIdentityServer.Server.Pages;

public class ConsentModel : PageModel
{
    private readonly ISender _sender;
    private readonly ITokenService _tokenService;

    public ConsentModel(ISender sender, ITokenService tokenService)
    {
        _sender = sender;
        _tokenService = tokenService;
    }

    [BindProperty(SupportsGet = true)] public string? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Scope { get; set; }
    [BindProperty(SupportsGet = true)] public string? State { get; set; }
    [BindProperty(SupportsGet = true)] public string? CodeChallenge { get; set; }
    [BindProperty(SupportsGet = true)] public string? CodeChallengeMethod { get; set; }
    [BindProperty(SupportsGet = true)] public string? Nonce { get; set; }
    [BindProperty(SupportsGet = true)] public string? RedirectUri { get; set; }
    [BindProperty] public string? Subject { get; set; }
    [BindProperty] public bool Remember { get; set; }

    public Core.Models.User? AuthenticatedUser { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // Map query names to view model
        ClientId ??= Request.Query["client_id"].ToString();
        Scope ??= Request.Query["scope"].ToString();
        State ??= Request.Query["state"].ToString();
        CodeChallenge ??= Request.Query["code_challenge"].ToString();
        CodeChallengeMethod ??= Request.Query["code_challenge_method"].ToString();
        Nonce ??= Request.Query["nonce"].ToString();
        RedirectUri ??= Request.Query["redirect_uri"].ToString();

        // If user already authenticated via cookie, load profile
        if (User?.Identity?.IsAuthenticated == true)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity.Name;
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    var resp = await _sender.Send(new Application.Queries.GetUserByEmailQuery(email), cancellationToken);
                    AuthenticatedUser = resp?.Data;
                }
                catch { }
            }
        }
    }

    public async Task<IActionResult> OnPostLoginAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(Subject))
        {
            ErrorMessage = "Please enter your email.";
            await OnGetAsync(cancellationToken);
            return Page();
        }

        // Validate user exists
        var resp = await _sender.Send(new Application.Queries.GetUserByEmailQuery(Subject), cancellationToken);
        var user = resp?.Data;
        if (user == null)
        {
            ErrorMessage = "User not found.";
            await OnGetAsync(cancellationToken);
            return Page();
        }

        // Create claims and sign in
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.Value.ToString()), new Claim(ClaimTypes.Email, user.Email ?? string.Empty) };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(id);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Redirect(Request.Path + Request.QueryString);
    }

    public async Task<IActionResult> OnPostLogoutAsync(CancellationToken cancellationToken)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(Request.Path + Request.QueryString);
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAuthorizeAsync(CancellationToken cancellationToken)
    {
        // Validate client id
        if (string.IsNullOrEmpty(ClientId))
        {
            ErrorMessage = "Missing client_id";
            await OnGetAsync(cancellationToken);
            return Page();
        }

        // Resolve application via application layer query
        Core.Models.Application? appModel = null;
        try
        {
            appModel = await _sender.Send(new Application.Application.Queries.GetApplicationByIdQuery(new Core.Models.ApplicationId(Guid.Parse(ClientId))), cancellationToken);
        }
        catch { }

        if (appModel == null)
        {
            ErrorMessage = "Invalid client";
            await OnGetAsync(cancellationToken);
            return Page();
        }

        // Use authenticated user if present
        var user = AuthenticatedUser;
        if (user == null && !string.IsNullOrEmpty(Subject))
        {
            var resp = await _sender.Send(new Application.Queries.GetUserByEmailQuery(Subject), cancellationToken);
            user = resp?.Data;
        }

        if (user == null)
        {
            ErrorMessage = "User not found.";
            await OnGetAsync(cancellationToken);
            return Page();
        }

        if (Remember)
        {
            await _sender.Send(new Application.Commands.CreateConsentCommand(new Core.Models.UserId(user.Id.Value), new Core.Models.ApplicationId(Guid.Parse(ClientId)), Scope ?? string.Empty, true), cancellationToken);
        }

        // Create authorization code and redirect to client
        var appGuid = Guid.Parse(ClientId);
        var code = await _tokenService.CreateAuthorizationCodeAsync(appGuid, user.Email ?? user.Id.Value.ToString(), Scope ?? string.Empty, DateTime.UtcNow.AddMinutes(5), CodeChallenge, CodeChallengeMethod, Nonce, cancellationToken);

        if (!string.IsNullOrEmpty(RedirectUri))
        {
            var uri = RedirectUri;
            var sep = uri.Contains('?') ? '&' : '?';
            var dest = uri + sep + "code=" + Uri.EscapeDataString(code);
            if (!string.IsNullOrEmpty(State)) dest += "&state=" + Uri.EscapeDataString(State);
            return Redirect(dest);
        }

        return Page();
    }
}
