using Movies.Api.Minimal.Auth;
using Movies.Api.Minimal.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Minimal.Endpoints.Movies;

public static class GetAllMoviesEndpoint
{
    public const string NameV1 = "GetAllMoviesV1";
    public const string NameV2 = "GetAllMoviesV2";

    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
            [AsParameters] GetAllMoviesRequest request, IMovieService movieService, HttpContext context, CancellationToken cancellationToken ) =>
        {
            var userId = context.GetUserId();
            var options = request.MapToOptions().WithUserId(userId);
            var movies = await movieService.GetAllAsync(options, cancellationToken);
            var movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
            var moviesResponse = movies.MapToResponse(request.Page.GetValueOrDefault(PagedRequest.DefaultPage)
                , request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize), movieCount);
            return TypedResults.Ok(moviesResponse);  
        })
        .WithName(NameV1)
        .Produces<MoviesResponse>(StatusCodes.Status200OK)
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(1.0)
        .RequireAuthorization();
        
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
                [AsParameters] GetAllMoviesRequest request, IMovieService movieService, HttpContext context, CancellationToken cancellationToken ) =>
            {
                var userId = context.GetUserId();
                var options = request.MapToOptions().WithUserId(userId);
                var movies = await movieService.GetAllAsync(options, cancellationToken);
                var movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
                var moviesResponse = movies.MapToResponse(request.Page.GetValueOrDefault(PagedRequest.DefaultPage)
                    , request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize), movieCount);
                return TypedResults.Ok(moviesResponse);  
            })
            .WithName(NameV2)
            .Produces<MoviesResponse>(StatusCodes.Status200OK)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(2.0)
            .CacheOutput("MovieCache")
            .RequireAuthorization();
        return app;
    }
}