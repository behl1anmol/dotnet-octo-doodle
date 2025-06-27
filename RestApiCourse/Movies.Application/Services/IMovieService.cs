using Movies.Application.Models;

namespace Movies.Application.Services;

//ideally we should be using a DTO object instead of using 
//a domain oject at this layer
//but for simplicity we are using the domain object here
//in a real world application we would use a DTO object
//eg. MovieDTO
public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie?> GetBySlugAsync(string slug);
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<Movie?> UpdateAsync(Movie movie);
    Task<bool> DeleteByIdAsync(Guid id);
}
