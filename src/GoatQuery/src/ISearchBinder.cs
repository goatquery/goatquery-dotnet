using System;
using System.Linq.Expressions;

public interface ISearchBinder<T>
{
    Expression<Func<T, bool>> Bind(string searchTerm);
}