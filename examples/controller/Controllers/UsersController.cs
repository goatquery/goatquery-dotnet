using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: /users
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [EnableQuery<UserDto>]
    public ActionResult<IEnumerable<UserDto>> Get()
    {
        var users = _userService.GetAllUsers();

        return Ok(users);
    }

    // GET: /users/alternative
    [HttpGet("alternative")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<UserDto>> GetAlternative([FromQuery] Query query)
    {
        var (users, _) = _userService.GetAllUsers().Apply(query);

        return Ok(users);
    }
}
