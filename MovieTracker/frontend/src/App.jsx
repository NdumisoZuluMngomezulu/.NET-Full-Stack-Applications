import { useCallback, useEffect, useRef, useState } from 'react';
import { api } from './api.js';
import Filters from './components/Filters.jsx';
import MovieForm from './components/MovieForm.jsx';
import MovieTicket from './components/MovieTicket.jsx';
import WatchDialog from './components/WatchDialog.jsx';

const DEFAULT_FILTERS = {
  search: '',
  status: 'all',
  genre: '',
  country: '',
  releasedFrom: '',
  releasedTo: '',
  watchedFrom: '',
  watchedTo: '',
  sort: 'added',
};

const isYear = (v) => /^\d{4}$/.test(v);

/** Converts the filter form state into the query string parameters the API expects. */
function toParams(f) {
  return {
    search: f.search.trim(),
    status: f.status === 'all' ? '' : f.status,
    genre: f.genre,
    country: f.country,
    releasedFrom: isYear(f.releasedFrom) ? `${f.releasedFrom}-01-01` : '',
    releasedTo: isYear(f.releasedTo) ? `${f.releasedTo}-12-31` : '',
    watchedFrom: f.watchedFrom,
    watchedTo: f.watchedTo,
    sort: f.sort,
  };
}

export default function App() {
  const [filters, setFilters] = useState(DEFAULT_FILTERS);
  const [movies, setMovies] = useState([]);
  const [facets, setFacets] = useState({ genres: [], countries: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  // null | { type: 'movie', movie? } | { type: 'watch', movie, viewing? }
  const [dialog, setDialog] = useState(null);
  const latestRequest = useRef(0);

  const loadMovies = useCallback(async (f) => {
    const id = ++latestRequest.current;
    try {
      const data = await api.list(toParams(f));
      if (id !== latestRequest.current) return; // a newer request superseded this one
      setMovies(data);
      setError('');
    } catch (e) {
      if (id === latestRequest.current) setError(e.message);
    } finally {
      if (id === latestRequest.current) setLoading(false);
    }
  }, []);

  const loadFacets = useCallback(async () => {
    try {
      setFacets(await api.facets());
    } catch {
      /* the list request will already be showing an error */
    }
  }, []);

  // Re-query shortly after the filters change so typing in the search box doesn't fire a request per key.
  useEffect(() => {
    const timer = setTimeout(() => loadMovies(filters), 250);
    return () => clearTimeout(timer);
  }, [filters, loadMovies]);

  useEffect(() => {
    loadFacets();
  }, [loadFacets]);

  const refresh = () => Promise.all([loadMovies(filters), loadFacets()]);

  // Runs an action from the page itself (not from a dialog) and reports any failure at the top.
  const run = async (action) => {
    try {
      await action();
      await refresh();
    } catch (e) {
      setError(e.message);
    }
  };

  // ----- dialog saves (they throw so the dialog can show the message) -----
  const saveMovie = async (input) => {
    if (dialog.movie) await api.update(dialog.movie.id, input);
    else await api.create(input);
    setDialog(null);
    await refresh();
  };

  const saveViewing = async (input) => {
    const { movie, viewing } = dialog;
    if (viewing) await api.updateViewing(movie.id, viewing.id, input);
    else await api.addViewing(movie.id, input);
    setDialog(null);
    await refresh();
  };

  // ----- actions from the cards -----
  const deleteMovie = (movie) => {
    if (!window.confirm(`Delete "${movie.title}" and its viewing history?`)) return;
    run(() => api.remove(movie.id));
  };

  const rateViewing = (movie, viewing, rating) =>
    run(() =>
      api.updateViewing(movie.id, viewing.id, {
        watchedOn: viewing.watchedOn,
        rating,
        notes: viewing.notes,
      }),
    );

  const removeViewing = (movie, viewing) => {
    const message =
      movie.watchCount === 1
        ? `Remove the only viewing of "${movie.title}"? It will go back to not watched.`
        : 'Remove this viewing?';
    if (!window.confirm(message)) return;
    run(() => api.removeViewing(movie.id, viewing.id));
  };

  // ----- summary -----
  const isDefault = JSON.stringify(filters) === JSON.stringify(DEFAULT_FILTERS);
  const watchedCount = movies.filter((m) => m.watched).length;
  const ratings = movies.map((m) => m.latestRating).filter((r) => r != null);
  const average = ratings.length
    ? (ratings.reduce((a, b) => a + b, 0) / ratings.length / 2).toFixed(1)
    : null;

  return (
    <div className="app">
      <header className="masthead">
        <div>
          <h1>Stubs</h1>
          <p>Keep the movies you love, tick off what you've seen, and rate it again next time.</p>
        </div>
        <button type="button" className="btn btn-primary" onClick={() => setDialog({ type: 'movie' })}>
          Add a movie
        </button>
      </header>

      <div className="layout">
        <Filters
          filters={filters}
          setFilters={setFilters}
          facets={facets}
          isDefault={isDefault}
          onReset={() => setFilters(DEFAULT_FILTERS)}
        />

        <main>
          {error && (
            <p className="error banner" role="alert">
              {error}
            </p>
          )}

          {!loading && movies.length > 0 && (
            <p className="summary" aria-live="polite">
              {movies.length} {movies.length === 1 ? 'movie' : 'movies'}, {watchedCount} watched
              {average && `, average rating ${average} of 5`}
            </p>
          )}

          {loading ? (
            <p className="empty">Loading your movies...</p>
          ) : movies.length === 0 ? (
            <div className="empty">
              {isDefault ? (
                <>
                  <p>Your list is empty. Add the first movie you want to keep track of.</p>
                  <button type="button" className="btn btn-primary" onClick={() => setDialog({ type: 'movie' })}>
                    Add a movie
                  </button>
                </>
              ) : (
                <>
                  <p>No movies match these filters.</p>
                  <button type="button" className="btn" onClick={() => setFilters(DEFAULT_FILTERS)}>
                    Clear filters
                  </button>
                </>
              )}
            </div>
          ) : (
            <div className="tickets">
              {movies.map((movie) => (
                <MovieTicket
                  key={movie.id}
                  movie={movie}
                  onMarkWatched={(m) => setDialog({ type: 'watch', movie: m })}
                  onEdit={(m) => setDialog({ type: 'movie', movie: m })}
                  onDelete={deleteMovie}
                  onEditViewing={(m, v) => setDialog({ type: 'watch', movie: m, viewing: v })}
                  onRateViewing={rateViewing}
                  onRemoveViewing={removeViewing}
                />
              ))}
            </div>
          )}
        </main>
      </div>

      {dialog?.type === 'movie' && (
        <MovieForm movie={dialog.movie} facets={facets} onSave={saveMovie} onClose={() => setDialog(null)} />
      )}
      {dialog?.type === 'watch' && (
        <WatchDialog
          movie={dialog.movie}
          viewing={dialog.viewing}
          onSave={saveViewing}
          onClose={() => setDialog(null)}
        />
      )}
    </div>
  );
}
