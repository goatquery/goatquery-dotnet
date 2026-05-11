using AutoMapper;
using AutoMapper.QueryableExtensions;
using GoatQuery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [EnableQuery<UserDto>(maxTop: 10)]
    public ActionResult<IEnumerable<UserDto>> Get()
    {
        var users = _db
            .Users.Include(x => x.Company)
            .Include(x => x.Addresses)
                .ThenInclude(x => x.City)
            .Include(x => x.Manager)
                .ThenInclude(x => x.Manager)
            .Include(x => x.Orders)
                .ThenInclude(x => x.Items)
                    .ThenInclude(x => x.Product)
            .Where(x => !x.IsDeleted)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider);

        return Ok(users);
    }
}
