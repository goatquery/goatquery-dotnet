using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("controller/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UsersController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // GET: /controller/users
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<UserDto>> Get()
    {
        var users = _db.Users
            .Where(x => !x.IsDeleted)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider);

        return Ok(users);
    }
}