# Stubs: a movie log

Save the movies you care about, tick off the ones you've watched, rate them, and watch (and rate) them again.

- **Front end:** React 18 + Vite (`frontend/`)
- **Back end:** ASP.NET Core Web API, C#, Entity Framework Core (`backend/MovieTracker.Api/`)
- **Database:** SQLite. It's a single file (`movies.db`) created automatically on first run, so there is nothing to install or configure.

## What it does

- Add, edit and delete movies: title, release date, genres, country, director, poster link, synopsis.
- **Tick off** a movie as watched with a date, a rating and optional notes.
- **Rate** from half a star to five stars. Click a star on a ticket to re-rate straight away.
- **Rewatch:** "Watch again" logs another viewing. Every viewing keeps its own date, rating and notes, so you can see how your opinion changes. The ticket shows the latest rating and the average.
- **Filter** by release year range, date watched (range), genre, country and watched / not yet, plus search and sorting.

## Run it (development)

You need the [.NET SDK](https://dotnet.microsoft.com/download) (the project targets `net10.0`) and Node.js 18+.

```bash
# 1. API (http://localhost:5080)
cd backend/MovieTracker.Api
dotnet run

# 2. React app (http://localhost:5173), in a second terminal
cd frontend
npm install
npm run dev
```

Open http://localhost:5173. Vite proxies `/api` calls to the API, so no CORS setup is needed.

If you have .NET 8 or 9, change `<TargetFramework>` in `MovieTracker.Api.csproj` (for example `net8.0`) and the EF Core package version (`8.0.*`).

## Run it (single server)

```bash
cd frontend
npm install
npm run build        # writes the site into backend/MovieTracker.Api/wwwroot
cd ../backend/MovieTracker.Api
dotnet run           # serves the site and the API on http://localhost:5080
```

## API

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/movies` | List movies. Query: `search`, `genre`, `country`, `status` (`watched`/`unwatched`), `releasedFrom`, `releasedTo`, `watchedFrom`, `watchedTo` (all `yyyy-MM-dd`), `sort` (`added`, `title`, `released`, `watched`, `rating`) |
| GET | `/api/movies/facets` | Distinct genres and countries for the filter dropdowns |
| GET | `/api/movies/{id}` | One movie with its viewings |
| POST | `/api/movies` | Add a movie |
| PUT | `/api/movies/{id}` | Edit a movie |
| DELETE | `/api/movies/{id}` | Delete a movie and its viewings |
| POST | `/api/movies/{id}/viewings` | Tick off / watch again: `{ watchedOn, rating, notes }` |
| PUT | `/api/movies/{id}/viewings/{viewingId}` | Edit a viewing or re-rate it |
| DELETE | `/api/movies/{id}/viewings/{viewingId}` | Remove a viewing (the last one un-ticks the movie) |

Ratings are integers 1 to 10, shown as 0.5 to 5 stars.

## Design notes

- `Movie` has many `Viewing`s. "Watched" simply means a movie has at least one viewing, which is what makes rewatching and re-rating natural: nothing is overwritten.
- Genres are stored as a comma-separated string and normalised to title case on save, which keeps the schema to two tables while still allowing exact-match filtering.
- `Database.EnsureCreated()` keeps first run simple. Before deploying anywhere real, switch to EF Core migrations.
- There are no user accounts, so everything is one shared list. To make it per-user, add ASP.NET Core Identity and give `Movie` an `OwnerId`.
