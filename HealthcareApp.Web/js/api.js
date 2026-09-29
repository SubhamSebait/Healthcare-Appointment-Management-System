/**
 * api.js - Central API helper for Healthcare App
 * Handles fetch calls, JWT headers, and error handling.
 */

const API_BASE = 'https://healthcare-appointment-api-2nxo.onrender.com/api';

// ── Token Management ────────────────────────────────────────────
function getToken()    { return localStorage.getItem('token'); }
function getUser()     { return JSON.parse(localStorage.getItem('user') || 'null'); }
function getRole()     { return localStorage.getItem('role'); }
function getProfileId(){ return parseInt(localStorage.getItem('profileId') || '0'); }

function saveAuth(data) {
    localStorage.setItem('token',     data.token);
    localStorage.setItem('role',      data.role);
    localStorage.setItem('profileId', data.profileId ?? '');
    localStorage.setItem('user', JSON.stringify({ fullName: data.fullName, userId: data.userId }));
}

function logout() {
    localStorage.clear();
    window.location.href = '/index.html';
}

// ── Redirect Guards ─────────────────────────────────────────────
function requireAuth(expectedRole) {
    const token = getToken();
    const role  = getRole();
    if (!token) { window.location.href = '/index.html'; return; }
    if (expectedRole && role !== expectedRole) {
        alert('Access denied. Wrong role.');
        logout();
    }
}

// ── Core Fetch ──────────────────────────────────────────────────
async function apiFetch(path, method = 'GET', body = null, auth = true) {
    const headers = { 'Content-Type': 'application/json' };
    if (auth) headers['Authorization'] = `Bearer ${getToken()}`;

    const options = { method, headers };
    if (body) options.body = JSON.stringify(body);

    const res = await fetch(`${API_BASE}${path}`, options);

    if (res.status === 401) { logout(); return; }

    let data;
    try { data = await res.json(); } catch { data = {}; }

    if (!res.ok) {
        const msg = data.message || data.title || `Error ${res.status}`;
        throw new Error(msg);
    }
    return data;
}

// ── Auth APIs ───────────────────────────────────────────────────
const Auth = {
    login: (email, password) => apiFetch('/auth/login', 'POST', { email, password }, false),
    register: (dto) => apiFetch('/auth/register', 'POST', dto, false),
};

// ── Doctor APIs ─────────────────────────────────────────────────
const Doctors = {
    getAll: (specializationId) => {
        const q = specializationId ? `?specializationId=${specializationId}` : '';
        return apiFetch(`/doctors${q}`, 'GET', null, false);
    },
    getById: (id) => apiFetch(`/doctors/${id}`, 'GET', null, false),
};

// ── Specialization APIs ─────────────────────────────────────────
const Specializations = {
    getAll:  ()        => apiFetch('/specializations', 'GET', null, false),
    create:  (dto)     => apiFetch('/specializations', 'POST', dto),
    update:  (id, dto) => apiFetch(`/specializations/${id}`, 'PUT', dto),
    delete:  (id)      => apiFetch(`/specializations/${id}`, 'DELETE'),
};

// ── Availability APIs ───────────────────────────────────────────
const Availability = {
    getFree:    (doctorId) => apiFetch(`/availability/${doctorId}`, 'GET', null, false),
    getAll:     (doctorId) => apiFetch(`/availability/${doctorId}/all`),
    addSlot:    (dto)      => apiFetch('/availability', 'POST', dto),
    deleteSlot: (id)       => apiFetch(`/availability/${id}`, 'DELETE'),
};

// ── Appointment APIs ────────────────────────────────────────────
const Appointments = {
    book:        (dto) => apiFetch('/appointments', 'POST', dto),
    getMy:       ()    => apiFetch('/appointments/my'),
    cancel:      (id)  => apiFetch(`/appointments/${id}/cancel`, 'PATCH'),
    getDoctor:   ()    => apiFetch('/appointments/doctor'),
    confirm:     (id, doctorNotes) => apiFetch(`/appointments/${id}/confirm`, 'PATCH', { doctorNotes }),
    reject:      (id, doctorNotes) => apiFetch(`/appointments/${id}/reject`,  'PATCH', { doctorNotes }),
    complete:    (id, doctorNotes) => apiFetch(`/appointments/${id}/complete`, 'PATCH', { doctorNotes }),
    getAll:      ()    => apiFetch('/appointments/all'),
};

// ── Admin APIs ──────────────────────────────────────────────────
const Admin = {
    getDoctors:       ()        => apiFetch('/admin/doctors'),
    addDoctor:        (dto)     => apiFetch('/admin/doctors', 'POST', dto),
    updateDoctor:     (id, dto) => apiFetch(`/admin/doctors/${id}`, 'PUT', dto),
    deleteDoctor:     (id)      => apiFetch(`/admin/doctors/${id}`, 'DELETE'),
    getPatients:      ()        => apiFetch('/admin/patients'),
};

// ── UI Helpers ──────────────────────────────────────────────────
function showToast(message, type = 'info') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        document.body.appendChild(container);
    }
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    container.appendChild(toast);
    setTimeout(() => toast.remove(), 3500);
}

function getStatusBadge(status) {
    const classes = {
        Pending:   'badge-pending',
        Confirmed: 'badge-confirmed',
        Rejected:  'badge-rejected',
        Cancelled: 'badge-cancelled',
        Completed: 'badge-completed',
    };
    return `<span class="badge ${classes[status] || ''}">${status}</span>`;
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}

function formatTime(timeStr) {
    if (!timeStr) return '';
    const [h, m] = timeStr.split(':');
    const d = new Date();
    d.setHours(+h, +m);
    return d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
}

function getInitials(name) {
    return name ? name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase() : '?';
}

function setupSidebar() {
    const user = getUser();
    const role = getRole();
    const nameEl = document.getElementById('sidebar-name');
    const roleEl = document.getElementById('sidebar-role');
    if (nameEl) nameEl.textContent = user?.fullName || 'User';
    if (roleEl) roleEl.textContent = role || '';

    document.querySelectorAll('.nav-item').forEach(item => {
        if (item.href && window.location.pathname.endsWith(item.getAttribute('href'))) {
            item.classList.add('active');
        }
    });
}
