import { useState } from 'react';
import Modal from './Modal.jsx';
import StarRating from './StarRating.jsx';
import { todayIso } from '../utils.js';

/**
 * Used for three jobs:
 *  - first tick-off        (movie not watched yet, no viewing)
 *  - watching it again     (movie already watched, no viewing)
 *  - editing a viewing     (re-rate, change the date or notes)
 */
export default function WatchDialog({ movie, viewing, onSave, onClose }) {
  const [watchedOn, setWatchedOn] = useState(viewing?.watchedOn ?? todayIso());
  const [rating, setRating] = useState(viewing?.rating ?? null);
  const [notes, setNotes] = useState(viewing?.notes ?? '');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const title = viewing ? 'Edit viewing' : movie.watched ? 'Watch again' : 'Mark as watched';

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      await onSave({ watchedOn, rating, notes });
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <Modal title={title} onClose={onClose}>
      <p className="dialog-movie">{movie.title}</p>
      <form className="form-grid" onSubmit={submit}>
        <label className="field">
          <span>Date watched</span>
          <input
            type="date"
            value={watchedOn}
            max={todayIso()}
            onChange={(e) => setWatchedOn(e.target.value)}
            required
          />
        </label>

        <div className="field">
          <span>Your rating</span>
          <div className="rating-row">
            <StarRating value={rating} onChange={setRating} size={32} />
            <span className="rating-value">{rating ? `${rating / 2} of 5` : 'Not rated'}</span>
          </div>
        </div>

        <label className="field">
          <span>Notes</span>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={3}
            maxLength={1000}
            placeholder="Who you watched it with, what stood out..."
          />
        </label>

        {error && <p className="error" role="alert">{error}</p>}

        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving...' : viewing ? 'Save changes' : 'Save viewing'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
