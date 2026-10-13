const STATUS_OPTIONS = [
  { value: 'all', label: 'All' },
  { value: 'watched', label: 'Watched' },
  { value: 'unwatched', label: 'Not yet' },
];

const SORT_OPTIONS = [
  { value: 'added', label: 'Recently added' },
  { value: 'title', label: 'Title A to Z' },
  { value: 'released', label: 'Newest release' },
  { value: 'watched', label: 'Recently watched' },
  { value: 'rating', label: 'Highest rated' },
];

export default function Filters({ filters, setFilters, facets, isDefault, onReset }) {
  const set = (field) => (e) => setFilters((f) => ({ ...f, [field]: e.target.value }));

  return (
    <aside className="filters" aria-label="Filter movies">
      <label className="field">
        <span>Search</span>
        <input
          type="search"
          value={filters.search}
          onChange={set('search')}
          placeholder="Title or director"
        />
      </label>

      <div className="field">
        <span id="status-label">Show</span>
        <div className="segmented" role="group" aria-labelledby="status-label">
          {STATUS_OPTIONS.map((o) => (
            <button
              key={o.value}
              type="button"
              aria-pressed={filters.status === o.value}
              onClick={() => setFilters((f) => ({ ...f, status: o.value }))}
            >
              {o.label}
            </button>
          ))}
        </div>
      </div>

      <label className="field">
        <span>Genre</span>
        <select value={filters.genre} onChange={set('genre')}>
          <option value="">Any genre</option>
          {facets.genres.map((g) => (
            <option key={g} value={g}>
              {g}
            </option>
          ))}
        </select>
      </label>

      <label className="field">
        <span>Country</span>
        <select value={filters.country} onChange={set('country')}>
          <option value="">Any country</option>
          {facets.countries.map((c) => (
            <option key={c} value={c}>
              {c}
            </option>
          ))}
        </select>
      </label>

      <fieldset className="field">
        <legend>Release year</legend>
        <div className="row-2">
          <input
            type="number"
            inputMode="numeric"
            min="1888"
            max="2100"
            placeholder="From"
            aria-label="Released from year"
            value={filters.releasedFrom}
            onChange={set('releasedFrom')}
          />
          <input
            type="number"
            inputMode="numeric"
            min="1888"
            max="2100"
            placeholder="To"
            aria-label="Released up to year"
            value={filters.releasedTo}
            onChange={set('releasedTo')}
          />
        </div>
      </fieldset>

      <fieldset className="field">
        <legend>Watched between</legend>
        <div className="stack">
          <input
            type="date"
            aria-label="Watched from date"
            value={filters.watchedFrom}
            onChange={set('watchedFrom')}
          />
          <input
            type="date"
            aria-label="Watched up to date"
            value={filters.watchedTo}
            onChange={set('watchedTo')}
          />
        </div>
      </fieldset>

      <label className="field">
        <span>Sort by</span>
        <select value={filters.sort} onChange={set('sort')}>
          {SORT_OPTIONS.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      </label>

      <button type="button" className="btn" onClick={onReset} disabled={isDefault}>
        Clear filters
      </button>
    </aside>
  );
}
