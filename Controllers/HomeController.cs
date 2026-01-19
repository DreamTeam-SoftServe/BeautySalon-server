using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/")] // відповідає на /
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("API is running!");
}
