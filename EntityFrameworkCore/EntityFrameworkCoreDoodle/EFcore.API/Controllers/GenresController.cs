using EFcore.API.Data;
using EFcore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFcore.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GenresController : Controller
{
    private readonly MoviesContext _context;

    public GenresController(MoviesContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<Genre>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Genres.ToListAsync());
    }

    [HttpGet("by-ids")]
    [ProducesResponseType(typeof(List<Genre>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllByIds([FromQuery] List<int> ids)
    {
        if (ids == null || !ids.Any())
            return BadRequest("No genre IDs provided.");

        var genres = await _context.Genres.Where(g => ids.Contains(g.Id)).ToListAsync();
        return Ok(genres);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Genre), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var genre = await _context.Genres.FindAsync(id);

        if(genre == null)
            return NotFound();
#if DEBUG
        //accessing and logging shadow property
        var createdDate = _context.Entry<Genre>(genre!).Property<DateTime>("CreatedDate").CurrentValue;
        //log the created date
        Console.WriteLine($"Genre created date: {createdDate}");
#endif      
        return Ok(genre);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Genre), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] Genre genre)
    {
        await _context.Genres.AddAsync(genre);
        
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = genre.Id }, genre);
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Genre), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Genre genre)
    {
        var existingGenre = await _context.Genres.FindAsync(id);

        if (existingGenre is null)
            return NotFound();

        existingGenre.Name = genre.Name;

        await _context.SaveChangesAsync();

        return Ok(existingGenre);
    }
    
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove([FromRoute] int id)
    {
        var existingGenre = await _context.Genres.FindAsync(id);

        if (existingGenre is null)
            return NotFound();

        _context.Genres.Remove(existingGenre);

        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpGet("from-query")]
    [ProducesResponseType(typeof(IEnumerable<Genre>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFromQuery()
    {
        var minimumGenreId = 2;
        //using from sql query also prevents from SQL injection
        //we can also use linq with fromsql
        var genres = await _context.Genres
            .FromSql($"SELECT * FROM [dbo].[Genres] WHERE ID >= {minimumGenreId}")
            .Where(genre=>genre.Name != "Comedy")
            .ToListAsync();
        
        //the below method FromSqlRaw does not prevent from SQL injection
        //it treats the query as a string literal
        var genresRawSQL = await _context.Genres
            .FromSqlRaw($"SELECT * FROM [dbo].[Genres] WHERE ID >= {minimumGenreId}")
            .ToListAsync();
        
        //the proper way of using fromsqlraw is below
        //this will protect from SQL injection attack
        var genresFromRawSQL = await _context.Genres
            .FromSqlRaw("SELECT * FROM [dbo].[Genres] WHERE ID >= {0}",minimumGenreId)
            .ToListAsync();
        return Ok(genres);       
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<GenreName>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNames()
    {
        // a good way to work with keyless entity availavle .net 8 and above
        var names = await _context.Database
            .SqlQuery<GenreName>($"SELECT Name FROM [dbo].[Genres]")
            .ToListAsync();
        return Ok(names);       
    }
    
    [HttpPut("batch-update")]
    [ProducesResponseType(typeof(IEnumerable<Genre>), StatusCodes.Status201Created)]
    public async Task<IActionResult> UpdateAll([FromBody] IEnumerable<Genre>? genres)
    {
        if (genres is null || !genres.Any())
        {
            return BadRequest("No genres provided for update.");
        }

        var updatedGenres = new List<Genre>();
        
        //once we have fetched everything from the database, any sunsequent 
        //find will result in hitting the EF cache
        var existingGenres = this.GetAllByIds(genres.Select(g => g.Id).ToList());
        
        foreach (var genre in genres)
        {
            var existingGenre = await _context.Genres.FindAsync(genre.Id);
            if (existingGenre != null)
            {
                existingGenre.Name = genre.Name;
                updatedGenres.Add(existingGenre);
            }
        }

        await _context.SaveChangesAsync();
        
        return Ok(updatedGenres);
    }
    
}