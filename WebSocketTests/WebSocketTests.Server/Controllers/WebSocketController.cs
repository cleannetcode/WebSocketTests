using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebSocketTests.Server.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;

    public WebSocketController(ILogger<WebSocketController> logger)
    {
        _logger = logger;
    }

    [Authorize]
    [HttpGet("/ws")]
    public async Task Get()
    {
        const string featureName = "echo";
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation(
                "request received, {state}, {status}, {description}",
                webSocket.State,
                webSocket.CloseStatus,
                webSocket.CloseStatusDescription);

            var userName = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            var key = $"{userName}{featureName}";

            try
            {
                await TestModule.Echo(webSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}