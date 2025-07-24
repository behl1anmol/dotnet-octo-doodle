using EFcore.API.Data.EntityMapping;
using EFcore.API.Data.Interceptors;
using EFcore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EFcore.API.Data;

public class MoviesContext : DbContext
{
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<ExternalInformation> ExternalInformation => Set<ExternalInformation>();
    public DbSet<Actor> Actors => Set<Actor>();
    //public DbSet<GenreName> GenreNames => Set<GenreName>();

    public MoviesContext(DbContextOptions<MoviesContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MovieMapping());
        modelBuilder.ApplyConfiguration(new GenreMapping());
        modelBuilder.ApplyConfiguration(new ExternalInformationMapping());
        modelBuilder.ApplyConfiguration(new ActorMapping());

        // modelBuilder.Entity<Movie>() //entity builder
        //     .ToTable("Pictures")
        //     .HasKey(m => m.Identifier);

        // modelBuilder.Entity<Movie>().Property(movie => movie.Title) //property builder
        //     .HasColumnType("varchar")
        //     .HasMaxLength(128)
        //     .IsRequired();

        // modelBuilder.Entity<Movie>().Property(movie => movie.ReleaseDate) //property builder
        //     .HasColumnType("date");

        // modelBuilder.Entity<Movie>().Property(movie => movie.Synopsis) //property builder
        //     .HasColumnType("varchar(max)")
        //     .HasColumnName("Plot");

        //refer genre controller for more cleaner way of doing the below
        // modelBuilder.Entity<GenreName>()
        //     .HasNoKey() //this is a keyleentity so do not change track
        //     //.ToView() //we can also register a keyless entity for a view in DB 
        //     .ToSqlQuery($"SELECT Name FROM [dbo].[Genres]");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new SaveChangesInterceptor());
    }
}