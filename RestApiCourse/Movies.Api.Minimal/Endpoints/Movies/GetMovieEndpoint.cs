using Movies.Api.Minimal.Auth;
using Movies.Api.Minimal.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Api.Minimal.Endpoints.Movies;

public static class GetMovieEndpoint
{
    public const string Name = "GetMovie";

    public static IEndpointRouteBuilder MapGetMovie(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.Get, async (
            string idOrSlug, IMovieService movieService, HttpContext context, CancellationToken cancellationToken ) =>
        {
            var userId = context.GetUserId();
            //we are using a slug here but still the actual identifier is Id
            var movie = Guid.TryParse(idOrSlug, out var id) ?
                await movieService.GetByIdAsync(id, userId, cancellationToken)
                : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
            //Never return a domain object from the API always return a contract
            if (movie is null)
            {
                return Results.NotFound();
            }

            var response = movie.MapToResponse();
            return TypedResults.Ok(response);   
        })
        .WithName(Name)
        .Produces<MovieResponse>(StatusCodes.Status200OK)
        .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
        .CacheOutput("MovieCache")
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(1.0)
        .RequireAuthorization();
        return app;
    }
}