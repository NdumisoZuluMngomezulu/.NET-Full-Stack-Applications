import { useState } from 'react';
import Modal from './Modal.jsx';

export default function MovieForm({ movie, facets, onSave, onClose }) {
  const [form, setForm] = useState({
    title: movie?.title ?? '',
    releaseDate: movie?.releaseDate ?? '',
    genres: movie?.genres?.join(', ') ?? '',
    country: movie?.country ?? '',
    director: movie?.director ?? '',
    posterUrl: movie?.posterUrl ?? '',
    synopsis: movie?.synopsis ?? '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  const addGenre = (genre) => {
    const current = form.genres.split(',').map((g) => g.trim()).filter(Boolean);
    if (current.some((g) => g.toLowerCase() === genre.toLowerCase())) return;
    setForm((f) => ({ ...f, genres: [...current, genre].join(', ') }));
  };

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      await onSave({
        title: form.title,
        releaseDate: form.releaseDate || null,
        genres: form.genres.split(',').map((g) => g.trim()).filter(Boolean),
        country: form.country,
        director: form.director,
        posterUrl: form.posterUrl,
        synopsis: form.synopsis,
      });
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <Modal title={movie ? 'Edit movie' : 'Add a movie'} onClose={onClose}>
      <form className="form-grid" onSubmit={submit}>
        <label className="field">
          <span>Title</span>
          <input value={form.title} onChange={set('title')} required maxLength={200} autoFocus />
        </label>

        <div className="row-2">
          <label className="field">
            <span>Release date</span>
            <input type="date" value={form.releaseDate} onChange={set('releaseDate')} />
          </label>
          <label className="field">
            <span>Country</span>
            <input
              value={form.country}
              onChange={set('country')}
              list="country-options"
              maxLength={100}
              placeholder="e.g. South Korea"
            />
            <datalist id="country-options">
              {facets.countries.map((c) => (
                <option key={c} value={c} />
              ))}
            </datalist>
          </label>
        </div>

        <label className="field">
          <span>Genres (separate with commas)</span>
          <input value={form.genres} onChange={set('genres')} placeholder="Drama, Thriller" />
          {facets.genres.length > 0 && (
            <span className="suggestions">
              {facets.genres.map((g) => (
                <button type="button" key={g} className="chip chip-button" onClick={() => addGenre(g)}>
                  {g}
                </button>
              ))}
            </span>
          )}
        </label>

        <label className="field">
          <span>Director</span>
          <input value={form.director} onChange={set('director')} maxLength={200} />
        </label>

        <label className="field">
          <span>Poster image link</span>
          <input
            type="url"
            value={form.posterUrl}
            onChange={set('posterUrl')}
            placeholder="https://..."
          />
        </label>

        <label className="field">
          <span>Synopsis</span>
          <textarea value={form.synopsis} onChange={set('synopsis')} rows={3} maxLength={2000} />
        </label>

        {error && <p className="error" role="alert">{error}</p>}

        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving...' : movie ? 'Save changes' : 'Add movie'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
