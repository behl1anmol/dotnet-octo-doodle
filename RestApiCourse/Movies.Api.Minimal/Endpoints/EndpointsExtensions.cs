using Movies.Api.Minimal.Endpoints.Movies;
using Movies.Api.Minimal.Endpoints.Ratings;

namespace Movies.Api.Minimal.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMovieEndpoints();
        app.MapRatingEndpoints();
        return app;
    }
}