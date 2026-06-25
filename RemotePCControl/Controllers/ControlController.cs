using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemotePCControl.Models;
using RemotePCControl.Services;

namespace RemotePCControl.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ControlController : ControllerBase
{
    private readonly ICommandRouter _router;
    private readonly ILogger<ControlController> _logger;

    public ControlController(ICommandRouter router, ILogger<ControlController> logger)
    {
        _router = router;
        _logger = logger;
    }

    [HttpPost("command")]
    public ActionResult<CommandResponse> ExecuteCommand([FromBody] CommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Command))
            return BadRequest(new CommandResponse(false, "命令不能为空", request.Command ?? string.Empty, DateTime.Now));

        var result = _router.Execute(request.Command);
        return result;
    }

    [HttpGet("status")]
    public ActionResult<StatusResponse> GetStatus()
    {
        return _router.GetStatus();
    }

    [HttpGet("help")]
    public ActionResult<string> Help() => _router.GetHelp();
}
