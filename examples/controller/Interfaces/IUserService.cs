public interface IUserService
{
    IQueryable<UserDto> GetAllUsers();
}