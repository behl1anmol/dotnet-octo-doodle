using EFcore.API.Data.ValueGenerators;
using EFcore.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFcore.API.Data.EntityMapping;

public class GenreMapping : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasData(new Genre { Id = 1, Name = "Action" });

        //created date is a shadow property
        //it is not part of the model class
        //it is not mapped to any property in the model class
        //it is only used to store the created date in the database
        builder.Property<DateTime>("CreatedDate")
            .HasColumnName("CreatedAt")
            //generating value on the SQL side by specifying a function
            .HasDefaultValueSql("getdate()");
            //Generating value on the client side
            //this will only run when the entity is added to the context with unassigned value
            //does not work for seeded data or migrated data
            //only works when adding an entity to the context
            //.HasValueGenerator<CreatedDateGenerator>();

    }
}