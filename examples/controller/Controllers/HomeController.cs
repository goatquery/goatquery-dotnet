using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

public class HomeController : ControllerBase
{
    [HttpGet("")]
    public ActionResult Get()
    {
        return Redirect("/swagger/index.html");
    }
}