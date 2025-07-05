namespace Movies.Api.Minimal.Endpoints.Movies;

public static class MovieEndpointExtensions
{
    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        //we can also create groups like
        //this will change the routing and endpoints will be routed like /movies/*
        // var group = app.MapGroup("movies");
        // group.MapCreateMovie();
        // group.MapGetMovie();
        // group.MapGetAllMovies();
        // group.MapUpdateMovie();
        // group.MapDeleteMovie();
        
        app.MapCreateMovie();
        app.MapGetMovie();
        app.MapGetAllMovies();
        app.MapUpdateMovie();
        app.MapDeleteMovie();
        return app;
    }
}