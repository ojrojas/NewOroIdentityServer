using Microsoft.AspNetCore.Mvc.RazorPages;

public class ConsentModel : PageModel
{
    public Guid UserId { get; set; }
    public Guid ClientId { get; set; }
    public string? Scopes { get; set; }

    public void OnGet([FromQuery] Guid? userId, [FromQuery] Guid? clientId, [FromQuery] string? scopes)
    {
        UserId = userId ?? Guid.Empty;
        ClientId = clientId ?? Guid.Empty;
        Scopes = scopes;
    }
}
