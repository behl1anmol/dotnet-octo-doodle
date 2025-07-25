using EFcore.API.Data;
using EFcore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EFcore.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : Controller
{
    private readonly MoviesContext _context;

    public MoviesController(MoviesContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _context.Movies
            //.Include(m => m.Actors) //eager loading actors
            .AsNoTracking()
            .ToListAsync();
        
        //explicit loading with Actors
        foreach (var m in movies.Where(mv => mv.ImdbRating > 4))
        {
            await _context.Entry(m)
                .Collection(mv => mv.Actors)
                .LoadAsync();
        }
        
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var movie = await _context.Movies
            .Include(m => m.Genre)
            .SingleOrDefaultAsync(m => m.Identifier == id);
        if (movie == null)
        {
            return NotFound();
        }
        return Ok(movie);       
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] Movie movie)
    {
        await _context.Movies.AddAsync(movie);
        
        //movie has no ID
        await _context.SaveChangesAsync();
        //movie has an ID
        
        return CreatedAtAction(nameof(Get), new {id = movie.Identifier}, movie);       
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Movie movie)
    {
        var existingMovie = await _context.Movies.FindAsync(id);
        
        if(existingMovie is null)
        {
            return NotFound();
        }

        existingMovie.Title = movie.Title;
        existingMovie.ReleaseDate = movie.ReleaseDate;
        existingMovie.Synopsis = movie.Synopsis;
        
        await _context.SaveChangesAsync();
        
        return Ok(existingMovie);       
    }
    
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove([FromRoute] int id)
    {
        var existingMovie = await _context.Movies.FindAsync(id);
        
        if(existingMovie is null)
        {
            return NotFound();
        }
        
        _context.Movies.Remove(existingMovie);
        //_context.Remove(existingMovie);
        //if we do not want to return the deleted object we can leverage the below
        //_context.Movies.Remove(new Movie {Id = id});
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpGet("by-year/{year:int}")]
    [ProducesResponseType(typeof(List<MovieTitle>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByYear([FromRoute] int year)
    {
        //lambda syntax
        var filteredTitles = await _context.Movies
                                            .Where(m => m.ReleaseDate.Year == year)
                                            .Select(m => new MovieTitle {Id = m.Identifier, Title = m.Title})
                                            .ToListAsync();

        //query syntax
        var filteredTitlesQuery = from movie in _context.Movies
            where movie.ReleaseDate.Year == year
            select new MovieTitle {Id = movie.Identifier, Title = movie.Title};

        return Ok(filteredTitles);       
    } 
    
    private static readonly Func<MoviesContext,AgeRating,IEnumerable<MovieTitle>> CompiledQuery = EF.CompileQuery(
        (MoviesContext context, AgeRating rating) => 
            context.Movies
                .Where(m => m.AgeRating <= rating)
                .Select(m => new MovieTitle {Id = m.Identifier, Title = m.Title}));
    
    [HttpGet("until-age{ageRating}")]
    [ProducesResponseType(typeof(List<MovieTitle>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUntilAge([FromRoute] AgeRating ageRating)
    {
        // var filteredTitles = await _context.Movies
        //     .Where(m => m.AgeRating <= ageRating)
        //     .Select(m => new MovieTitle {Id = m.Identifier, Title = m.Title})
        //     .ToListAsync();
        
        var filteredTitles = CompiledQuery(_context, ageRating).ToList();

        return Ok(filteredTitles);       
    } 
}