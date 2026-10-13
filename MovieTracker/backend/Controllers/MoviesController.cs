using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.Api.Data;
using MovieTracker.Api.Dtos;
using MovieTracker.Api.Models;

namespace MovieTracker.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController(AppDbContext db) : ControllerBase
{
    // ---------------------------------------------------------------- movies

    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> List([FromQuery] MovieQuery q)
    {
        IQueryable<Movie> query = db.Movies.AsNoTracking().Include(m => m.Viewings);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim().ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(s)
                                     || (m.Director != null && m.Director.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(q.Genre))
        {
            var needle = "," + q.Genre.Trim() + ",";
            query = query.Where(m => m.Genres != null && ("," + m.Genres + ",").Contains(needle));
        }

        if (!string.IsNullOrWhiteSpace(q.Country))
        {
            var country = q.Country.Trim();
            query = query.Where(m => m.Country == country);
        }

        switch (q.Status?.ToLowerInvariant())
        {
            case "watched":
                query = query.Where(m => m.Viewings.Any());
                break;
            case "unwatched":
                query = query.Where(m => !m.Viewings.Any());
                break;
        }

        if (q.ReleasedFrom is { } releasedFrom)
            query = query.Where(m => m.ReleaseDate >= releasedFrom);
        if (q.ReleasedTo is { } releasedTo)
            query = query.Where(m => m.ReleaseDate <= releasedTo);

        if (q.WatchedFrom is not null || q.WatchedTo is not null)
        {
            var from = q.WatchedFrom ?? DateOnly.MinValue;
            var to = q.WatchedTo ?? DateOnly.MaxValue;
            query = query.Where(m => m.Viewings.Any(v => v.WatchedOn >= from && v.WatchedOn <= to));
        }

        // A personal movie list is small, so sorting the DTOs in memory keeps the SQL simple.
        var movies = await query.ToListAsync();
        var dtos = movies.Select(ToDto);

        IEnumerable<MovieDto> sorted = q.Sort?.ToLowerInvariant() switch
        {
            "title" => dtos.OrderBy(d => d.Title, StringComparer.OrdinalIgnoreCase),
            "released" => dtos.OrderByDescending(d => d.ReleaseDate),
            "watched" => dtos.OrderByDescending(d => d.LastWatchedOn),
            "rating" => dtos.OrderByDescending(d => d.LatestRating),
            _ => dtos.OrderByDescending(d => d.AddedAt).ThenByDescending(d => d.Id),
        };

        return sorted.ToList();
    }

    /// <summary>Distinct genres and countries, used to fill the filter dropdowns.</summary>
    [HttpGet("facets")]
    public async Task<ActionResult<FacetsDto>> Facets()
    {
        var rows = await db.Movies.AsNoTracking()
            .Select(m => new { m.Genres, m.Country })
            .ToListAsync();

        var genres = rows
            .SelectMany(r => SplitGenres(r.Genres))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var countries = rows
            .Select(r => r.Country)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new FacetsDto(genres, countries);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDto>> Get(int id)
    {
        var movie = await db.Movies.AsNoTracking()
            .Include(m => m.Viewings)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null) return NotFound();
        return ToDto(movie);
    }

    [HttpPost]
    public async Task<ActionResult<MovieDto>> Create(MovieInput input)
    {
        if (ValidateMovie(input) is { } error) return BadRequest(new { error });

        var movie = new Movie();
        Apply(movie, input);
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = movie.Id }, ToDto(movie));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieDto>> Update(int id, MovieInput input)
    {
        var movie = await LoadWithViewings(id);
        if (movie is null) return NotFound();
        if (ValidateMovie(input) is { } error) return BadRequest(new { error });

        Apply(movie, input);
        await db.SaveChangesAsync();
        return ToDto(movie);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await db.Movies.FindAsync(id);
        if (movie is null) return NotFound();

        db.Movies.Remove(movie); // viewings are removed by the cascade
        await db.SaveChangesAsync();
        return NoContent();
    }

    // -------------------------------------------------------------- viewings
    // A viewing is one time you watched a movie. Ticking a movie off creates the first one,
    // watching it again adds another, and editing a viewing's rating is how you re-rate.

    [HttpPost("{id:int}/viewings")]
    public async Task<ActionResult<MovieDto>> AddViewing(int id, ViewingInput input)
    {
        var movie = await LoadWithViewings(id);
        if (movie is null) return NotFound();
        if (ValidateViewing(input) is { } error) return BadRequest(new { error });

        movie.Viewings.Add(new Viewing
        {
            WatchedOn = input.WatchedOn,
            Rating = input.Rating,
            Notes = Clean(input.Notes),
        });
        await db.SaveChangesAsync();
        return ToDto(movie);
    }

    [HttpPut("{id:int}/viewings/{viewingId:int}")]
    public async Task<ActionResult<MovieDto>> UpdateViewing(int id, int viewingId, ViewingInput input)
    {
        var movie = await LoadWithViewings(id);
        var viewing = movie?.Viewings.FirstOrDefault(v => v.Id == viewingId);
        if (movie is null || viewing is null) return NotFound();
        if (ValidateViewing(input) is { } error) return BadRequest(new { error });

        viewing.WatchedOn = input.WatchedOn;
        viewing.Rating = input.Rating;
        viewing.Notes = Clean(input.Notes);
        await db.SaveChangesAsync();
        return ToDto(movie);
    }

    [HttpDelete("{id:int}/viewings/{viewingId:int}")]
    public async Task<ActionResult<MovieDto>> DeleteViewing(int id, int viewingId)
    {
        var movie = await LoadWithViewings(id);
        var viewing = movie?.Viewings.FirstOrDefault(v => v.Id == viewingId);
        if (movie is null || viewing is null) return NotFound();

        db.Viewings.Remove(viewing);
        await db.SaveChangesAsync();
        return ToDto(movie);
    }

    // --------------------------------------------------------------- helpers

    private Task<Movie?> LoadWithViewings(int id) =>
        db.Movies.Include(m => m.Viewings).FirstOrDefaultAsync(m => m.Id == id);

    private static string? ValidateMovie(MovieInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title)) return "Title is required.";
        if (input.Title.Trim().Length > 200) return "Title must be 200 characters or fewer.";
        if (input.Synopsis is { Length: > 2000 }) return "Synopsis must be 2000 characters or fewer.";

        if (!string.IsNullOrWhiteSpace(input.PosterUrl))
        {
            var ok = Uri.TryCreate(input.PosterUrl.Trim(), UriKind.Absolute, out var uri)
                     && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!ok) return "Poster URL must start with http:// or https://.";
        }

        return null;
    }

    private static string? ValidateViewing(ViewingInput input)
    {
        if (input.WatchedOn == default) return "Watched date is required.";
        if (input.WatchedOn > DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
            return "Watched date can't be in the future.";
        if (input.Rating is < 1 or > 10) return "Rating must be between 1 and 10.";
        if (input.Notes is { Length: > 1000 }) return "Notes must be 1000 characters or fewer.";
        return null;
    }

    private static void Apply(Movie movie, MovieInput input)
    {
        movie.Title = input.Title!.Trim();
        movie.ReleaseDate = input.ReleaseDate;
        movie.Genres = JoinGenres(input.Genres);
        movie.Country = Clean(input.Country);
        movie.Director = Clean(input.Director);
        movie.PosterUrl = Clean(input.PosterUrl);
        movie.Synopsis = Clean(input.Synopsis);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] SplitGenres(string? genres) =>
        string.IsNullOrWhiteSpace(genres)
            ? []
            : genres.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Normalises genre names ("sci-fi " becomes "Sci-Fi") so filtering matches exactly.</summary>
    private static string? JoinGenres(string[]? genres)
    {
        if (genres is null) return null;

        var cleaned = genres
            .Where(g => g is not null)
            .Select(g => g.Replace(",", " ").Trim())
            .Where(g => g.Length > 0)
            .Select(g => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(g.ToLowerInvariant()))
            .Distinct()
            .ToList();

        return cleaned.Count == 0 ? null : string.Join(",", cleaned);
    }

    private static MovieDto ToDto(Movie m)
    {
        // Newest viewing first.
        var viewings = m.Viewings
            .OrderByDescending(v => v.WatchedOn)
            .ThenByDescending(v => v.Id)
            .Select(v => new ViewingDto(v.Id, v.WatchedOn, v.Rating, v.Notes))
            .ToList();

        var rated = viewings.Where(v => v.Rating.HasValue).ToList();

        int? latestRating = rated.Count > 0 ? rated[0].Rating : null;
        double? averageRating = rated.Count > 0
            ? Math.Round(rated.Average(v => v.Rating!.Value), 1)
            : null;
        DateOnly? lastWatchedOn = viewings.Count > 0 ? viewings[0].WatchedOn : null;

        return new MovieDto(
            m.Id,
            m.Title,
            m.ReleaseDate,
            SplitGenres(m.Genres),
            m.Country,
            m.Director,
            m.PosterUrl,
            m.Synopsis,
            m.AddedAt,
            viewings.Count > 0,
            viewings.Count,
            latestRating,
            averageRating,
            lastWatchedOn,
            viewings);
    }
}
