using Movies.Application.Models;

namespace Movies.Application.Services;

//ideally we should be using a DTO object instead of using 
//a domain oject at this layer
//but for simplicity we are using the domain object here
//in a real world application we would use a DTO object
//eg. MovieDTO
public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
