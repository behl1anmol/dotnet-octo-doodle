using EFcore.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFcore.API.Data.EntityMapping;

public class ExternalInformationMapping : IEntityTypeConfiguration<ExternalInformation>
{
    public void Configure(EntityTypeBuilder<ExternalInformation> builder)
    {
        builder.HasKey(info => info.MovieId);
    
        // we do have an option to configure a foreign key here
        // but EF will automatically assume the primary key as the foreign key
        builder.HasOne(info => info.Movie)
            .WithOne(movie => movie.ExternalInformation)
            .HasForeignKey<ExternalInformation>(info => info.MovieId);
    }
}