using EFcore.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFcore.API.Data.EntityMapping;

public class ActorMapping : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        //EF will create the joined entities for holding the foreign keys automatically
        builder.HasMany(actor => actor.Movies)
            .WithMany(movie => movie.ActorsList)
        // to get full control do this   
            .UsingEntity("MovieActor",
                left => left.HasOne(typeof(Movie))
                    .WithMany()
                    .HasForeignKey("MovieId")
                    .HasPrincipalKey(nameof(Movie.Identifier))
                    .HasConstraintName("FK_MovieActor_Movie")
                    .OnDelete(DeleteBehavior.Cascade),
                right => right.HasOne(typeof(Actor))
                    .WithMany()
                    .HasForeignKey("ActorId")
                    .HasPrincipalKey(nameof(Actor.Id))
                    .HasConstraintName("FK_MovieActor_Actor")
                    .OnDelete(DeleteBehavior.Cascade),
                linkBuilder => linkBuilder.HasKey("MovieId", "ActorId")
            ); 
    }
}