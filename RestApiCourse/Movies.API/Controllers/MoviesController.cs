using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.API.Auth;
using Movies.Contracts.Responses;
using Microsoft.AspNetCore.OutputCaching;

namespace Movies.API.Controllers;

[ApiController]
[ApiVersion(1.0)] 
[ApiVersion(2.0)] 
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly IOutputCacheStore _outputCacheStore;

    public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
    {
        _outputCacheStore = outputCacheStore;
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedorAdminUserPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, cancellationToken);
        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken); //removing the cache for movies tag

        //better implementation to return a response
        return CreatedAtAction(nameof(GetV1), new { idOrSlug = movie.Id }, movie);
        //Before
        //todo: return contracts
        //return Created($"/{ApiEndpoints.Movies.Create}{movie.Id}", movie);
    }

    [Authorize] // can add deprecated tag which will add a deprecation header to the response
    //[ApiVersion(1.0)] //add query string to the url to specify the version (Eg: api-version=1.0)
    [MapToApiVersion(1.0)] //this will map the action to the version 2.0
    [HttpGet(ApiEndpoints.Movies.Get)]
    [OutputCache]
    //for response caching we can use the ResponseCache attribute
    //[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)] //caching the response for 30 seconds
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetV1([FromRoute] string idOrSlug, [FromServices] LinkGenerator linkGenerator, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        //we are using a slug here but still the actual identifier is Id
        var movie = Guid.TryParse(idOrSlug, out var id) ?
                            await _movieService.GetByIdAsync(id, userId, cancellationToken)
                            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
        //Never return a domain object from the API always return a contract
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();

        //Creating HATEOS links
        var movieObj = new { id = movie.Id };
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetV1), values: new { idOrSlug = movie.Id }),
            Rel = "self",
            Type = "GET"
        });

        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: movieObj),
            Rel = "self",
            Type = "PUT"
        });

        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: movieObj),
            Rel = "self",
            Type = "DELETE"
        });
        return Ok(response);
    }

    [Authorize] // can add deprecated tag which will add a deprecation header to the response
    //[ApiVersion(1.0)] //add query string to the url to specify the version (Eg: api-version=1.0)
    [MapToApiVersion(2.0)] //this will map the action to the version 2.0
    [HttpGet(ApiEndpoints.Movies.Get)]
    [OutputCache]
    //[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)] //caching the response for 30 seconds
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetV2([FromRoute] string idOrSlug, [FromServices] LinkGenerator linkGenerator, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        //we are using a slug here but still the actual identifier is Id
        var movie = Guid.TryParse(idOrSlug, out var id) ?
                            await _movieService.GetByIdAsync(id, userId, cancellationToken)
                            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
        //Never return a domain object from the API always return a contract
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();

        //Creating HATEOS links
        var movieObj = new { id = movie.Id };
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetV2), values: new { idOrSlug = movie.Id }),
            Rel = "self",
            Type = "GET"
        });

        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: movieObj),
            Rel = "self",
            Type = "PUT"
        });

        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: movieObj),
            Rel = "self",
            Type = "DELETE"
        });
        return Ok(response);
    }

    [Authorize]
    //[AllowAnonymous] //for accessing without the token
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [OutputCache(PolicyName = "MovieCache")] //this will use the output cache policy defined in the AddOutputCache method
    // for response caching we can use the ResponseCache attribute
    // this will cache the response for 30 seconds and vary by query parameters
    //[ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "title", "year", "sortBy", "pageSize", "page" }, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)] //caching the response for 30 seconds
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions().WithUserId(userId);
        var movies = await _movieService.GetAllAsync(options, cancellationToken);
        var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
        var moviesResponse = movies.MapToResponse(request.Page, request.PageSize, movieCount);
        return Ok(moviesResponse);
    }

    [Authorize(AuthConstants.TrustedorAdminUserPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] Guid id,
    [FromBody] UpdateMovieRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie(id);
        var updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);
        if (updatedMovie == null)
        {
            return NotFound();
        }
        var response = updatedMovie.MapToResponse();
        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken); //removing the cache for movies tag
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _movieService.DeleteByIdAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken); //removing the cache for movies tag

        return Ok();
    }

    //why partial updates are not used?
    //due to complexity of creating a request. Constructing on the client is complicating. Handling on server 
    //is also complicating.
    //as a simple way we can get and update
}
