namespace MovieTracker.Api.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateOnly? ReleaseDate { get; set; }

    /// <summary>Comma-separated genre names, e.g. "Drama,Thriller".</summary>
    public string? Genres { get; set; }

    public string? Country { get; set; }
    public string? Director { get; set; }
    public string? PosterUrl { get; set; }
    public string? Synopsis { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Every time the movie was watched. A movie is "watched" when it has at least one viewing;
    /// rewatching adds another viewing, and each viewing carries its own rating.
    /// </summary>
    public List<Viewing> Viewings { get; set; } = [];
}
