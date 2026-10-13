import { useState } from 'react';
import StarRating from './StarRating.jsx';
import { formatDate, initials, releaseYear, starsText } from '../utils.js';

export default function MovieTicket({
  movie,
  onMarkWatched,
  onEdit,
  onDelete,
  onEditViewing,
  onRateViewing,
  onRemoveViewing,
}) {
  const [showHistory, setShowHistory] = useState(false);
  const [posterFailed, setPosterFailed] = useState(false);

  const facts = [releaseYear(movie.releaseDate), movie.country, movie.director && `Directed by ${movie.director}`]
    .filter(Boolean)
    .join(', ');
  const latest = movie.viewings[0];

  return (
    <article className={`ticket${movie.watched ? ' is-watched' : ''}`}>
      <div className="ticket-poster">
        {movie.posterUrl && !posterFailed ? (
          <img src={movie.posterUrl} alt="" loading="lazy" onError={() => setPosterFailed(true)} />
        ) : (
          <span aria-hidden="true">{initials(movie.title)}</span>
        )}
      </div>

      <div className="ticket-main">
        <h3>{movie.title}</h3>
        {facts && <p className="facts">{facts}</p>}

        {movie.genres.length > 0 && (
          <ul className="genres" aria-label="Genres">
            {movie.genres.map((g) => (
              <li key={g} className="chip">
                {g}
              </li>
            ))}
          </ul>
        )}

        {movie.synopsis && <p className="synopsis">{movie.synopsis}</p>}

        <div className="ticket-actions">
          {movie.watched ? (
            <>
              <button type="button" className="btn btn-sm" onClick={() => onMarkWatched(movie)}>
                Watch again
              </button>
              <button
                type="button"
                className="btn btn-sm"
                aria-expanded={showHistory}
                onClick={() => setShowHistory((s) => !s)}
              >
                {showHistory ? 'Hide history' : `History (${movie.watchCount})`}
              </button>
            </>
          ) : (
            <button type="button" className="btn btn-sm btn-primary" onClick={() => onMarkWatched(movie)}>
              Mark as watched
            </button>
          )}
          <button type="button" className="btn btn-sm" onClick={() => onEdit(movie)}>
            Edit
          </button>
          <button type="button" className="btn btn-sm btn-danger" onClick={() => onDelete(movie)}>
            Delete
          </button>
        </div>

        {showHistory && (
          <ol className="history" aria-label={`Viewing history for ${movie.title}`}>
            {movie.viewings.map((v) => (
              <li key={v.id}>
                <div className="history-top">
                  <strong>{formatDate(v.watchedOn)}</strong>
                  <StarRating
                    value={v.rating}
                    size={16}
                    label={`Rating for ${formatDate(v.watchedOn)}`}
                    onChange={(rating) => onRateViewing(movie, v, rating)}
                  />
                </div>
                {v.notes && <p className="history-notes">{v.notes}</p>}
                <div className="history-actions">
                  <button type="button" className="link" onClick={() => onEditViewing(movie, v)}>
                    Edit
                  </button>
                  <button type="button" className="link link-danger" onClick={() => onRemoveViewing(movie, v)}>
                    Remove
                  </button>
                </div>
              </li>
            ))}
          </ol>
        )}
      </div>

      <div className="ticket-stub">
        {movie.watched ? (
          <>
            <span className="stamp">Watched</span>
            <StarRating
              value={latest.rating}
              size={20}
              label="Rating of latest viewing"
              onChange={(rating) => onRateViewing(movie, latest, rating)}
            />
            <p className="stub-line">
              {latest.rating ? `${starsText(latest.rating)} of 5` : 'Tap a star to rate'}
            </p>
            <p className="stub-line muted">
              {movie.watchCount === 1 ? 'Watched once' : `Watched ${movie.watchCount} times`}
              <br />
              Last on {formatDate(latest.watchedOn)}
              {movie.watchCount > 1 && movie.averageRating != null && (
                <>
                  <br />
                  Average {starsText(movie.averageRating)} of 5
                </>
              )}
            </p>
          </>
        ) : (
          <p className="stub-line muted">Not watched yet</p>
        )}
      </div>
    </article>
  );
}
