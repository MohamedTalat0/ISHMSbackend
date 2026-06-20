using ISHMS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IHubService _hubService;

    public TestController(IHubService hubService)
    {
        _hubService = hubService;
    }

    [HttpGet("send-alert")]
    public async Task<IActionResult> SendAlert()
    {
        await _hubService.SendAlertAsync(
            1,
            "test",
            "Doctor",
            "test",
            "test",
            "test"
        );

        return Ok();
    }
}