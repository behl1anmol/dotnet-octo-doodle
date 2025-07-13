using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Minimal.Auth;
using Movies.Api.Minimal.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Api.Minimal.Endpoints.Movies;

public static class DeleteMovieEndpoint
{
    public const string Name = "DeleteMovie";

    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete, async (
            Guid id, IMovieService movieService, IOutputCacheStore outputCacheStore, CancellationToken cancellationToken ) =>
        {
            var deleted = await movieService.DeleteByIdAsync(id, cancellationToken);
            if (!deleted)
            {
                return Results.NotFound();
            }

            await outputCacheStore.EvictByTagAsync("movies", cancellationToken); //removing the cache for movies tag

            return TypedResults.Ok();
        })
        .WithName(Name)
        .Produces<MovieResponse>(StatusCodes.Status200OK)
        .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(1.0)
        .RequireAuthorization(AuthConstants.AdminUserPolicyName);;
        return app;
    }
}