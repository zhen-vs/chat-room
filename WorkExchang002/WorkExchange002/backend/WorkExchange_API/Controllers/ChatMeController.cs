using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WorkExchange_API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatMeController : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var raw =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("UserId");

        if (!int.TryParse(raw, out var id))
            return Unauthorized();

        var name =
            User.FindFirstValue(ClaimTypes.Name) ??
            User.Identity?.Name ??
            "";

        var roleRaw = User.FindFirstValue(ClaimTypes.Role);

        var role = roleRaw switch
        {
            "1" => "Worker",
            "2" => "Host",
            "3" => "Admin",
            _ => "Unknown"
        };

        return Ok(new { id, role, name });
    }
}
