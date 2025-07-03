using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Auth;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.API.Controllers.V1;

[ApiController]
public class RatingsController(IRatingService _ratingService) : ControllerBase
{
    [Authorize]
    [HttpPut(ApiEndpoints.V1.Movies.Rate)]
    public async Task<IActionResult> RateMovie([FromRoute]Guid movieId, [FromBody]RateMovieRequest rating, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.RateMovieAsync(movieId, rating.Rating, userId.Value, cancellationToken);
        return result? Ok() : NotFound();
    }
    
    [Authorize]
    [HttpDelete(ApiEndpoints.V1.Movies.DeleteRating)]
    public async Task<IActionResult> DeleteRating([FromRoute]Guid movieId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.DeleteRatingAsync(movieId, userId.Value, cancellationToken);
        return result? Ok() : NotFound();
    }
    
    [Authorize]
    [HttpGet(ApiEndpoints.V1.Ratings.GetUserRatings)]
    public async Task<IActionResult> GetUserRatings(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var ratings = await _ratingService.GetRatingsForUserAsync(userId.Value, cancellationToken);
        var ratingsRepsponse = ratings.MapToResponse();
        return Ok(ratingsRepsponse);
        
    }
}

