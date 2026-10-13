/** Parses "YYYY-MM-DD" as a local date, avoiding the off-by-one-day timezone shift of new Date(iso). */
function parseIsoDate(iso) {
  const [y, m, d] = iso.split('-').map(Number);
  return new Date(y, m - 1, d);
}

export function formatDate(iso) {
  if (!iso) return '';
  return parseIsoDate(iso).toLocaleDateString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

export function releaseYear(iso) {
  return iso ? iso.slice(0, 4) : '';
}

export function todayIso() {
  const d = new Date();
  const pad = (n) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

/** Ratings are stored 1-10 (half-star steps) and shown as 0.5-5 stars. */
export function starsText(rating) {
  return rating == null ? '' : String(rating / 2);
}

export function initials(title) {
  return title
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0].toUpperCase())
    .join('');
}
