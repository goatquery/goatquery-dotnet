using System;
using System.Linq.Expressions;

/// <summary>
/// Defines custom search logic for a given entity type.
/// Implement this interface and register it with DI to enable the <c>search</c> query parameter.
/// </summary>
/// <typeparam name="T">The entity type to search.</typeparam>
public interface ISearchBinder<T>
{
    /// <summary>
    /// Returns a predicate expression that filters entities matching the given search term.
    /// </summary>
    /// <param name="searchTerm">The free-text search term from the query string.</param>
    Expression<Func<T, bool>> Bind(string searchTerm);
}
