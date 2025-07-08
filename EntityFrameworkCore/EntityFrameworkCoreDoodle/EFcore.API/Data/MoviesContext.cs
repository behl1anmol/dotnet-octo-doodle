using EFcore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EFcore.API.Data;

public class MoviesContext : DbContext
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Data Source=localhost;Initial Catalog=MoviesDB;User Id=sa;Password=MySaPassword123;TrustServerCertificate=True;");
        //not proper logging
        optionsBuilder.LogTo(Console.WriteLine);
        base.OnConfiguring(optionsBuilder);
    }
}