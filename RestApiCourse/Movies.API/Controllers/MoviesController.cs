using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.API.Auth;

namespace Movies.API.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedorAdminUserPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, cancellationToken);

        //better implementation to return a response
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);

        //Before
        //todo: return contracts
        //return Created($"/{ApiEndpoints.Movies.Create}{movie.Id}", movie);
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
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
        return Ok(response);
    }

    [Authorize]
    //[AllowAnonymous] //for accessing without the token
    [HttpGet(ApiEndpoints.Movies.GetAll)]
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
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _movieService.DeleteByIdAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return Ok();
    }

    //why partial updates are not used?
    //due to complexity of creating a request. Constructing on the client is complicating. Handling on server 
    //is also complicating.
    //as a simple way we can get and update
}
