using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Minimal.Auth;
using Movies.Api.Minimal.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Minimal.Endpoints.Movies;

public static class CreateMovieEndpoints
{
    public const string Name = "CreateMovie";
    
    public static IEndpointRouteBuilder MapCreateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Movies.Create,
            async (
                CreateMovieRequest request, IMovieService movieService, IOutputCacheStore outputCacheStore, CancellationToken cancellationToken) =>
            {
                var movie = request.MapToMovie();
                await movieService.CreateAsync(movie, cancellationToken);
                await outputCacheStore.EvictByTagAsync("movies", cancellationToken); //removing the cache for movies tag

                //better implementation to return a response
                var response = movie.MapToResponse();
                return TypedResults.CreatedAtRoute(response, GetMovieEndpoint.Name, new { idOrSlug = movie.Id });
            })
            .WithName(Name)
            .Produces<CreateMovieRequest>(StatusCodes.Status201Created)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0)
            .RequireAuthorization(AuthConstants.TrustedorAdminUserPolicyName);
        return app;
    }
}