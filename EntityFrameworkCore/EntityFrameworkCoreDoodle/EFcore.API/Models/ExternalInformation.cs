namespace EFcore.API.Models;

public class ExternalInformation
{
    //this property is added to serve both as a primary key and a foreign key
    //making sure that a movie may or may not have a reference to an external information
    //but an external information record will have a single movie or 
    //will refer to a single movie record
    public int MovieId { get; set; }
    public string? ImdbUrl { get; set; }
    public string? RottenTomatoesUrl { get; set; }
    public string? TmdbUrl { get; set; }
    
    public Movie Movie { get; set; }    
}