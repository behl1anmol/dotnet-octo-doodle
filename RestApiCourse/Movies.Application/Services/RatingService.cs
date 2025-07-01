using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository) : IRatingService
{
    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (rating is < 1 or > 5)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = "Rating",
                    ErrorMessage = "Rating must be between 1 and 5"
                }
            ]);
        }
        var movie = await movieRepository.ExistsByIdAsync(movieId, cancellationToken);
        if (!movie)
        {
            return false;
        }
        return await ratingRepository.RateMovieAsync(movieId, rating, userId, cancellationToken);
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await ratingRepository.DeleteRatingAsync(movieId, userId, cancellationToken);       
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await ratingRepository.GetRatingsForUserAsync(userId, cancellationToken);       
    }
}