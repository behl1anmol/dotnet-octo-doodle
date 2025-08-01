using System;
using EFcore.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFcore.API.Data.EntityMapping;

public class MovieMapping : IEntityTypeConfiguration<EFcore.API.Models.Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies")
            .HasQueryFilter(m=>m.ReleaseDate >= DateTime.Now.AddYears(-20));

        builder.HasKey(m => m.Identifier);
            //.IsClustered(false); had to remove this as migrations were not working properly
            // because this is a foreign key for movie_actors table
        
        //defining compound keys for an alternate key
        builder.HasAlternateKey(movie => new { movie.Title, movie.ReleaseDate }).IsClustered(false);
            //.IsClustered(true); had to change this to false to make migrations work properly
            //as we can have only one clustered index which is identifier

        builder.HasIndex(movie => movie.AgeRating)
            .IsDescending();

        builder.Property(movie => movie.Title)
            .HasColumnType("varchar")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(movie => movie.ReleaseDate)
            .HasColumnType("char(8)")
            .HasConversion(new DateTimeToChar8Converter());

        builder.Property(movie => movie.Synopsis)
            .HasColumnType("varchar(max)")
            .HasColumnName("Plot");
        
        builder.Property(movie => movie.MainGenreName)
            .HasMaxLength(256)
            .HasColumnType("varchar");
        
        //one-to-many relationship
        builder
            .HasOne(m => m.Genre)
            .WithMany(g => g.Movies)
            .HasPrincipalKey(g => g.Name) //define the unique identifier in the entity to be the foreign key
            .HasForeignKey(m => m.MainGenreName);

        builder
            .OwnsOne(movies => movies.Director)
            .ToTable("Movie_Director");
            //.ComplexProperty(movies => movies.Director);
        
        
            builder
                .OwnsMany(movies => movies.Actors)
                .ToTable("Movie_Actors");

            #region DataSeed Coommented because included in migrations

            //while providing seed for owned property we need to provide data for all the columns in the table
            //except the primary key
            // builder
            //     .OwnsOne(movies => movies.Director)
            //     .ToTable("Movie_Director")
            //     .HasData(new { MovieIdentifier = 1, FirstName = "John", LastName = "Doe" });
            //     //.ComplexProperty(movies => movies.Director);
            //
            //
            //     builder
            //         .OwnsMany(movies => movies.Actors)
            //         .ToTable("Movie_Actors")
            //         .HasData(
            //             new { MovieIdentifier = 1, Id = 1, FirstName = "Edward", LastName = "Nortan" },
            //             new { MovieIdentifier = 1, Id = 2, FirstName = "Brad", LastName = "Pitt" });
            //
            // //Seed - data that needs to be created always
            // builder.HasData(new Movie
            // {
            //     Identifier = 1,
            //     Title = "The Matrix",
            //     ReleaseDate = new DateTime(1999, 3, 31),
            //     Synopsis = "The Matrix is a 1999 science fiction action film directed by the Academy Award-winning screenwriter <NAME> and starring <NAME>, <NAME>, <NAME>, <NAME> and <NAME>.",
            //     MainGenreId = 1,
            //     AgeRating = AgeRating.Adolescent
            // });

            #endregion

            #region mapping enum as string can cause issues

            //using the below mapping will cause EF to do a comparison between strings
            //Therefore, we need to handle this situation in our query accordingly
            //8/07/2025 23:45:31.692 RelationalEventId.CommandExecuting[20100] (Microsoft.EntityFrameworkCore.Database.Command) 
            // // Executing DbCommand [Parameters=[@__ageRating_0='?' (Size = 32) (DbType = AnsiString)], CommandType='Text', CommandTimeout='30']
            // // SELECT [m].[Identifier] AS [Id], [m].[Title]
            // // FROM [Movies] AS [m]
            // // WHERE [m].[AgeRating] <= @__ageRating_0
            // builder.Property(movie => movie.AgeRating)
            //     .HasColumnType("varchar(32)")
            //     .HasConversion<string>();

            #endregion

    }
}
