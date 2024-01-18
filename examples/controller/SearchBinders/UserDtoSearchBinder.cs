using System.Linq.Expressions;

public class UserDtoSearchBinder : ISearchBinder<UserDto>
{
    public Expression<Func<UserDto, bool>> Bind(string searchTerm)
    {
        Expression<Func<UserDto, bool>> exp = x => x.Firstname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || x.Lastname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

        return exp;
    }
}