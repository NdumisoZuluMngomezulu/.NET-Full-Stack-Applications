namespace MovieTracker.Api.Dtos;

// ----- Requests -----

public record MovieInput(
    string? Title,
    DateOnly? ReleaseDate,
    string[]? Genres,
    string? Country,
    string? Director,
    string? PosterUrl,
    string? Synopsis);

public record ViewingInput(DateOnly WatchedOn, int? Rating, string? Notes);

/// <summary>Query-string filters for GET /api/movies. Everything is optional.</summary>
public class MovieQuery
{
    public string? Search { get; set; }
    public string? Genre { get; set; }
    public string? Country { get; set; }

    /// <summary>"watched" or "unwatched". Anything else returns both.</summary>
    public string? Status { get; set; }

    /// <summary>Release date range (inclusive).</summary>
    public DateOnly? ReleasedFrom { get; set; }
    public DateOnly? ReleasedTo { get; set; }

    /// <summary>Matches movies with at least one viewing inside this range (inclusive).</summary>
    public DateOnly? WatchedFrom { get; set; }
    public DateOnly? WatchedTo { get; set; }

    /// <summary>added (default), title, released, watched, rating.</summary>
    public string? Sort { get; set; }
}

// ----- Responses -----

public record ViewingDto(int Id, DateOnly WatchedOn, int? Rating, string? Notes);

public record MovieDto(
    int Id,
    string Title,
    DateOnly? ReleaseDate,
    string[] Genres,
    string? Country,
    string? Director,
    string? PosterUrl,
    string? Synopsis,
    DateTime AddedAt,
    bool Watched,
    int WatchCount,
    int? LatestRating,
    double? AverageRating,
    DateOnly? LastWatchedOn,
    List<ViewingDto> Viewings);

public record FacetsDto(List<string> Genres, List<string> Countries);
