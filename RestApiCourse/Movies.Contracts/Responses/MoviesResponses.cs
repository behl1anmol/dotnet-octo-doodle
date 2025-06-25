using System;

namespace Movies.Contracts.Responses;

public class MoviesResponses
{
    public required IEnumerable<MovieResponse> Items { get; init; } = Enumerable.Empty<MovieResponse>();
}
