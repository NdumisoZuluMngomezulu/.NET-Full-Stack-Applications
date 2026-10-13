namespace MovieTracker.Api.Models;

public class Viewing
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public DateOnly WatchedOn { get; set; }

    /// <summary>1-10, where 10 = five stars (each half star is one step). Null = not rated yet.</summary>
    public int? Rating { get; set; }

    public string? Notes { get; set; }
}
