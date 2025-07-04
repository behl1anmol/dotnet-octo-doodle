using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;

//var moviesApi = RestService.For<IMoviesApi>("https://localhost:5001");

var services = new ServiceCollection();

services
.AddHttpClient()
.AddSingleton<AuthTokenProvider>()
.AddRefitClient<IMoviesApi>(x => new RefitSettings
{
    AuthorizationHeaderValueGetter = async (request, cancellationToken) => await x.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
}).ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:5001"));

var provider = services.BuildServiceProvider();


var moviesApi = provider.GetRequiredService<IMoviesApi>();
var movie = await moviesApi.GetMovieAsync("nick-the-greek-2022");

Console.WriteLine(JsonSerializer.Serialize(movie));

var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
    Title = "Spiderman 2",
    YearOfRelease = 2002,
    Genres = new List<string> { "Action", "Drama" }
});

await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest
{
    Title = "Spiderman 2",
    YearOfRelease = 2022,
    Genres = new List<string> { "Action", "Drama", "Thriller" }
});

await moviesApi.DeleteMovieAsync(newMovie.Id);

var request = new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    PageSize = 10,
    Page = 1
};

var movies = await moviesApi.GetMoviesAsync(request);
Console.WriteLine(JsonSerializer.Serialize(movies, new JsonSerializerOptions { WriteIndented = true }));
