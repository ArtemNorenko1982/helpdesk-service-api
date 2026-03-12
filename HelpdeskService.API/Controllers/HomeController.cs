using Asp.Versioning;
using Helpdesk.Service.Library;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        var source = new SourceClass
        {
            Id = 1,
            FullName = "John Doe",
            IsOutDated = true,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var target = ObjectMapper.Map<SourceClass, TargetClass>(source);

        return Ok(new
        {
            Message = "Welcome to the HelpDesk Service API",
            Version = "1.0",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
