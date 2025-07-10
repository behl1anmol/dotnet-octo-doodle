using System;
using System.Text.Json.Serialization;

namespace EFcore.API.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // [JsonIgnore]
    // public DateTime CreatedDate { get; set; }
    
    //this is dont to break cyclic dependency
    [JsonIgnore]
    public ICollection<Movie> Movies
    {
        get;
        set;
    } = new  HashSet<Movie>();
}
