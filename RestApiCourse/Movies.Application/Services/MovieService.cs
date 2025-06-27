using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<Movie> _movieValidator;

    public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator)
    {
        _movieValidator = movieValidator;
        _movieRepository = movieRepository;
    }
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        //this type of errors in api must be 400 errors 
        //therefore we will be throwing an exception
        //and handle it on the api layer
        await _movieValidator.ValidateAndThrowAsync(movie);
        return await _movieRepository.CreateAsync(movie);
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        return _movieRepository.DeleteByIdAsync(id);
    }

    public Task<IEnumerable<Movie>> GetAllAsync()
    {
        return _movieRepository.GetAllAsync();
    }

    public Task<Movie?> GetByIdAsync(Guid id)
    {
        return _movieRepository.GetByIdAsync(id);
    }

    public Task<Movie?> GetBySlugAsync(string slug)
    {
        return _movieRepository.GetBySlugAsync(slug);
    }

    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        //this type of errors in api must be 400 errors 
        //therefore we will be throwing an exception
        //and handle it on the api layer
        await _movieValidator.ValidateAndThrowAsync(movie);
        var movieExists = _movieRepository.ExistsByIdAsync(movie.Id);
        if (!await movieExists)
        {
            return null; // Movie does not exist, return null
        }

        await _movieRepository.UpdateAsync(movie);
        return movie;
    }
}
