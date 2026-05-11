using System.Linq.Expressions;
using GoatQuery;

public class UserSearchBinder : ISearchBinder<UserDto>
{
    public Expression<Func<UserDto, bool>> Bind(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return u =>
            u.Firstname.ToLower().Contains(term)
            || u.Lastname.ToLower().Contains(term)
            || (u.Company != null && u.Company.Name.ToLower().Contains(term));
    }
}
