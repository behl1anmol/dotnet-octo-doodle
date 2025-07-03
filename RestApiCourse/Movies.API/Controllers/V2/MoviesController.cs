using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.API.Auth;
using Movies.Contracts.Responses;

namespace Movies.API.Controllers.V2;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedorAdminUserPolicyName)]
    [HttpPost(ApiEndpoints.V2.Movies.Create)]
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
    [HttpGet(ApiEndpoints.V2.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, [FromServices] LinkGenerator linkGenerator, CancellationToken cancellationToken)
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
}
