using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IValidator<Movie> _movieValidator;
    private readonly IValidator<GetAllMoviesOptions> _optionsValidator;

    public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator, IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> getAllMoviesOptionsValidator)
    {
        _ratingRepository = ratingRepository;
        _movieValidator = movieValidator;
        _movieRepository = movieRepository;
        _optionsValidator = getAllMoviesOptionsValidator;
    }
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        //this type of errors in api must be 400 errors 
        //therefore we will be throwing an exception
        //and handle it on the api layer
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _movieRepository.DeleteByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
    {
        await _optionsValidator.ValidateAndThrowAsync(options, cancellationToken);
        return await _movieRepository.GetAllAsync(options, cancellationToken);
    }

    public Task<Movie?> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetByIdAsync(id, userid, cancellationToken);
    }

    public Task<Movie?> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetBySlugAsync(slug, userid, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userid = default, CancellationToken cancellationToken = default)
    {
        //this type of errors in api must be 400 errors 
        //therefore we will be throwing an exception
        //and handle it on the api layer
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        var movieExists = _movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!await movieExists)
        {
            return null; // Movie does not exist, return null
        }

        await _movieRepository.UpdateAsync(movie, cancellationToken);

        if (!userid.HasValue)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;
            return movie;
        }
        var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userid.Value, cancellationToken);
        movie.Rating = ratings.Rating;
        movie.UserRating = ratings.UserRating;
        return movie;
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease,
        CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetCountAsync(title, yearOfRelease, cancellationToken);
    }
}
