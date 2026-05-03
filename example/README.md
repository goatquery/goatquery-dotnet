# GoatQuery .NET Example

A sample ASP.NET Core API demonstrating GoatQuery features: filtering, ordering, pagination, search, and lambda expressions on collections.

## Prerequisites

- .NET 9 SDK
- Docker (for Postgres via Testcontainers)

## Running

```bash
cd example
dotnet run
```

The app starts a Postgres container automatically and seeds 1,000 users with orders, products, addresses, and companies.

## Endpoints

| Endpoint | Description |
|---|---|
| `GET /controller/users` | Controller-based with `[EnableQuery]` (maxTop: 10) |
| `GET /minimal/users` | Minimal API with manual `Apply()` |

Both endpoints support the same query parameters.

## Query Parameters

| Parameter | Description |
|---|---|
| `filter` | Filter expression |
| `orderby` | Order by expression |
| `top` | Number of results to return |
| `skip` | Number of results to skip |
| `count` | Set to `true` to include total count |
| `search` | Free-text search (requires ISearchBinder) |

### Search

The `/controller/users` endpoint has an `ISearchBinder<UserDto>` registered that searches across firstname, lastname, and company name:

```
?search=john
?search=acme
```
