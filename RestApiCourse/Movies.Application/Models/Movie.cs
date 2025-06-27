using System;
using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required int YearOfRelease { get; set; }
    public required List<string> Genres { get; init; } = new();
    public string Slug => GenerateSlug();

    /// <summary>
    /// Generating a slug which can be used to retrive the records.
    /// </summary>
    /// <returns></returns>
    private string GenerateSlug()
    {
        var sluggedTitle = SlugRegex().Replace(Title, string.Empty)
                            .ToLower().Replace(" ", "-");


        return $"{sluggedTitle}-{YearOfRelease}";
    }

    [GeneratedRegex("[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 5)]
    private static partial Regex SlugRegex();
}
