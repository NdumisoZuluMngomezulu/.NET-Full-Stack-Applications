const BASE = '/api/movies';

async function request(url, options = {}) {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });

  if (!res.ok) {
    let message = `Something went wrong (${res.status}). Check that the API is running.`;
    try {
      const body = await res.json();
      if (body?.error) message = body.error;
    } catch {
      /* response had no JSON body */
    }
    throw new Error(message);
  }

  return res.status === 204 ? null : res.json();
}

function toQueryString(params) {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== '' && value != null) search.set(key, value);
  });
  return search.toString();
}

const json = (method, body) => ({ method, body: JSON.stringify(body) });

export const api = {
  list: (filters) => request(`${BASE}?${toQueryString(filters)}`),
  facets: () => request(`${BASE}/facets`),
  create: (movie) => request(BASE, json('POST', movie)),
  update: (id, movie) => request(`${BASE}/${id}`, json('PUT', movie)),
  remove: (id) => request(`${BASE}/${id}`, { method: 'DELETE' }),

  addViewing: (movieId, viewing) => request(`${BASE}/${movieId}/viewings`, json('POST', viewing)),
  updateViewing: (movieId, viewingId, viewing) =>
    request(`${BASE}/${movieId}/viewings/${viewingId}`, json('PUT', viewing)),
  removeViewing: (movieId, viewingId) =>
    request(`${BASE}/${movieId}/viewings/${viewingId}`, { method: 'DELETE' }),
};
