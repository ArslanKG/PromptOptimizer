using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PromptOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("health")]
        public IActionResult Health() => Ok(new
        {
            status = "healthy",
            service = "AI Chat API",
            version = "2.0",
            timestamp = DateTime.UtcNow
        });

        [AllowAnonymous]
        [HttpGet("info")]
        public IActionResult Info() => Ok(new
        {
            name = "AI Chat API",
            version = "2.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}