using EFcore.API.Data.EntityMapping;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MovieMapping());

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
    }
}