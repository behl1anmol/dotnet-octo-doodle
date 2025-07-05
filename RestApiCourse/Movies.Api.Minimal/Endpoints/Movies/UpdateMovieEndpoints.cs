using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Minimal.Auth;
using Movies.Api.Minimal.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Minimal.Endpoints.Movies;

public static class UpdateMovieEndpoints
{
    public const string Name = "UpdateMovie";
    
    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update,
            async (
                Guid id, UpdateMovieRequest request
                , IMovieService movieService, HttpContext context
                , IOutputCacheStore outputCacheStore, CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var movie = request.MapToMovie(id);
                var updatedMovie = await movieService.UpdateAsync(movie, userId, cancellationToken);
                if (updatedMovie == null)
                {
                    return Results.NotFound();
                }
                var response = updatedMovie.MapToResponse();
                await outputCacheStore.EvictByTagAsync("movies", cancellationToken); //removing the cache for movies tag
                return TypedResults.Ok(response);
            })
            .WithName(Name)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .Produces<MovieResponse>(StatusCodes.Status200OK)
            .RequireAuthorization(AuthConstants.TrustedorAdminUserPolicyName);;
        return app;
    }
}