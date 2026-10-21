// page elements
const form = document.getElementById('add-form');
const titleInput = document.getElementById('title');
const dateInput = document.getElementById('date');
const assigneeInput = document.getElementById('assignee');
const message = document.getElementById('message');
const list = document.getElementById('tickets');
const filterButtons = document.querySelectorAll('[data-filter]');

// state
let tickets = [];
let filter = 'all'; // all, to clear, cleared

// server communication

// sends request to API. Throws error with a readabl
async function api(url, options = {}) {
    const response = await fetch(url, {
        headers: { 'Content-Type': 'application/json' },
        ...options,
    });

    if (!response.ok) {
        let text = 'Something went wrong. Is the server running';
        try {
            const body = await response.json();
            if (body.error) text = body.error;
        } catch {
            //no body
        }
        throw new Error(text);
    }

    return response.status === 204 ? null : response.json();
}

async function loadTickets() {
    try {
        tickets = await('/api/tickets');
        message.textContent = '';
        render();
    } catch (error) {
        message.textContent = error.message;
    }
}

// rendering the list

function render() {
    list.innerHTML = '';

    const visible = tickets.filter((ticket) => {
        if (filter === 'cleared') return ticket.cleared;
        if (filter === 'unwatched') return !ticket.cleared;
        return true;
    });

    if (visible.length === 0) {
        const empty = document.createElement('li');
        empty.className = 'empty';
        empty.textContent = tickets.length === 0
            ? 'No tickets yet. Add one above.'
            : 'No ticket in this view.';
        list.appendChild(empty);
        return;
    }

    visible.forEach((ticket) => list.appendChild(createTicketRow(ticket)));
}

function createTicketRow(ticket) {
  const row = document.createElement('li');
  if (ticket.watched) row.classList.add('watched');

  // Checkbox: tick the ticket off
  const checkbox = document.createElement('input');
  checkbox.type = 'checkbox';
  checkbox.checked = ticket.watched;
  checkbox.setAttribute('aria-label', `Mark ${ticket.title} as watched`);
  checkbox.addEventListener('change', () => updateWatched(ticket, checkbox.checked, null));

  // Title and details (textContent is used instead of innerHTML so typed text can't inject HTML)
  const info = document.createElement('div');
  const title = document.createElement('div');
  title.className = 'title';
  title.textContent = ticket.title;
  const meta = document.createElement('div');
  meta.className = 'meta';
  meta.textContent = [ticket.year, ticket.genre].filter(Boolean).join(', ');
  info.append(title, meta);

  row.append(checkbox, info);

  // Rating: only shown once the ticket has been watched
  if (ticket.watched) {
    const select = document.createElement('select');
    select.setAttribute('aria-label', `Rating for ${ticket.title}`);
    select.add(new Option('Rate it', ''));
    for (let stars = 1; stars <= 5; stars++) {
      select.add(new Option(stars === 1 ? '1 star' : `${stars} stars`, stars));
    }
    select.value = ticket.rating ?? '';
    select.addEventListener('change', () => {
      const rating = select.value === '' ? null : Number(select.value);
      updateWatched(ticket, true, rating);
    });
    row.appendChild(select);
  }

  // Delete button
  const remove = document.createElement('button');
  remove.type = 'button';
  remove.className = 'delete';
  remove.textContent = 'Delete';
  remove.addEventListener('click', () => deleteTicket(ticket));
  row.appendChild(remove);

  return row;
}

// ---- actions

async function deleteMovie(ticket) {
    if (!confirm(`Delete "${ticket.title}"?`)) return;
    try {
        await api(`/api/movies/${ticket.id}`, { method: 'DELETE'});
        await loadTickets();
    } catch (error) {
        message.textContent = error.message;
    }
}

async function deleteTickets(ticket) {
    if (!confirm(`Delete "${ticket.title}"?`)) return;
    try {
        await api(`/api/movies/${ticket.id}`, { method: 'DELETE' });
        await loadTickets();
    } catch (error) {
        message.textContent = error.message;
    }
}

form.addEventListener('submit', async (event) => {
    event.preventDefault();
    try {
        await api('/api/movies', {
            method: 'POST',
            body: JSON.stringify({
                title: titleInput.value,
                year: dateInput.value ? Number(dateInput.value) : null,
                assigne: assigneeInput.value,
            }),
        });
    } catch (error) {
        message.textContent = error.message;
    }
});

filterButtons.forEach((button) => {
  button.addEventListener('click', () => {
    filter = button.dataset.filter;
    filterButtons.forEach((b) => b.setAttribute('aria-pressed', String(b === button)));
    render();
  });
});

// ----- start -----
loadMovies();

