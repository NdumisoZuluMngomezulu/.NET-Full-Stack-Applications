const $ = (id) => document.getElementById(id);
let token = sessionStorage.getItem('novabank_token');
let authMode = 'login';
let myAccount = null;
let transactions = [];
const money = (n) => new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR'
}).format(Number(n || 0));

const api = async (url, options = {}) => {
    const response = await fetch(url, {
        ...options, headers: { 
            'Content-Type': 'application/json',
            ...(token ? {Authorization: `${token}` }: {}),
            ...(options.headers || {})
            }
    });
    const data = response.status === 204 ? null : response.json().catch(() => null);
    if (!response.ok) {const message = data?.message || data?.title || data?.errors?.join?.(' ') || (response.status === 401 ? 'Please sign in again.' : 'Request failed.'); throw new Error(message);}
    return data;
};

function setMessage(id, text, success = false) { 
    const el = $(id); 
    el.textContent = text; 
    el.classList.toggle('success', success); 
}

function notify(message) {
    const el = $('toast');
    el.textContent = message;
    el.classList.add('show');
    setTimeout(() => el.classList.remove('show'), 3200);
}

function setAuthMode(mode) {
    authMode = mode;
    document.querySelectorAll('[data-tab]').forEach(b => b.classList.toggle('active', b.dataset.tab === mode));
    $('name-field').classList.toggle('hidden', mode !== 'register');
    $('auth-submit').innerHTML = mode === 'login' ? 'Sign in securely <span>=></span>' : 'Create demo account <span>=></span>';
    $('password').autocomplete = mode === 'login' ? 'current-password' : 'new-password';
    setMessage('auth-message', '');
}

document.querySelectorAll('[data-tab]').forEach(b => b.addEventListener('click', () => setAuthMode(b.dataset.tab)));

$('auth-form').addEventListener('submit', async e => {
    e.preventDefault();
    const button = $('auth-submit');
    button.disabled = true;
    setMessage('auth-message', '');
    try {
        if (authMode === 'register') {
            await ('/api/auth/register', { method: 'POST', body: JSON.stringify({ fullName: $('fullName').value.trim(), email: $('email').value.trim(), password: $('password').value })});
        }
        const result = await api('/api/auth/login', { 
            method: 'POST', 
            body: JSON.stringify({ email: $('email').value.trim(), password: $('password').value })
        });
    } catch (err) {
        setMessage('auth-message', err.message);
    } finally {
        button.disabled = false;
    }
});

async function showDashboard() {
    $('auth-panel').classList.add('hidden');
    $('logout').classList.remove('hidden');
    $('profile-name').textContent = 'Your profile';
    await refreshDashboard();
}

async function refreshDashboard() { 
    try { 
        const [accounts, txs] = await Promise.all([api('/api/accounts/me'), api('/api/transactions')]); 
        myAccount = accounts[0]; 
        transactions = txs; $('balance').textContent = money(myAccount?.balance); $('account-number').textContent = myAccount ? `A/C ${myAccount.accountNumber}` : 'No account'; const outgoing = txs.filter(t => t.fromAccountId === myAccount?.id); const incoming = txs.filter(t => t.toAccountId === myAccount?.id); $('sent').textContent = money(outgoing.reduce((s,t) => s + t.amount, 0)); $('received').textContent = money(incoming.reduce((s,t) => s + t.amount, 0)); renderTransactions(); } catch (err) { notify(err.message); if (/sign in/i.test(err.message)) logout(); } }

