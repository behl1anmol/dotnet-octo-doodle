namespace EFcore.API.Models;

public class Movie
{
    public int Identifier { get; set; }
    public string? Title { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? Synopsis { get; set; }
    public int ImdbRating
    {
        get;
        set;
    }
    public AgeRating AgeRating { get; set; }
    
    public Person Director { get; set; }
    public ICollection<Person> Actors
    {
        get;
        set;
    }
    
    //navigation property will make the foreign key reference and create a property GenreID
    //we can also add genreid as a property
    public Genre Genre
    {
        get;
        set;
    }
    
    public int MainGenreId { get; set; }
}

public enum AgeRating
{
    All = 0,
    ElementarySchool = 6,
    HighSchool = 12,
    Adolescent = 16,
    Adult = 18
}

public class MovieTitle
{
    public string? Title { get; set; }
    public int Id { get; set; }
}