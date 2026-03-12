/* api.js — всі запити до бекенду
   Base URL: http://localhost:5000 */
const API_BASE = 'http://localhost:5000';

async function request(endpoint, options = {}) {
    const token = localStorage.getItem('gym_token');
    const headers = {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
        ...options.headers,
    };

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, { ...options, headers });

        if (res.status === 401) {
            localStorage.removeItem('gym_token');
            localStorage.removeItem('gym_user');
            showPage('login');
            showToast('Сесія закінчилась. Увійдіть знову.', 'error');
            return null;
        }

        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || `HTTP ${res.status}`);
        }

        if (res.status === 204) return true;
        return await res.json();
    } catch (e) {
        console.error(`[API] ${endpoint}:`, e.message);
        throw e;
    }
}

const CoachAPI = {
    getAll: () => request('/api/test-domain2/coaches'),
    getById: (id) => request(`/api/Coach/${id}`),
    getBySpecialization: (spec) => request(`/api/Coach/specialization/${encodeURIComponent(spec)}`),
    delete: (id) => request(`/api/Coach/${id}`, { method: 'DELETE' }),
};

const PlanAPI = {
    getAll: () => request('/api/MembershipPlan'),
    create: (data) => request('/api/MembershipPlan', { method: 'POST', body: JSON.stringify(data) }),
    delete: (id) => request(`/api/MembershipPlan/${id}`, { method: 'DELETE' }),
};

const ClassAPI = {
    getSchedule: (date) => request(`/api/Class/schedule/${date}`),
    getById: (id) => request(`/api/Class/${id}`),
    create: (data) => request('/api/Class', { method: 'POST', body: JSON.stringify(data) }),
    reschedule: (id, d) => request(`/api/Class/${id}/reschedule`, { method: 'PUT', body: JSON.stringify(d) }),
    getCoachEfficiency: () => request('/api/Class/analytics/coach-efficiency'),
};

const InvoiceAPI = {
    getPending: (clientId) => request(`/api/v1/invoices/pending/${clientId}`),
    create: (data) => request('/api/v1/invoices/create', { method: 'POST', body: JSON.stringify(data) }),
    pay: (invoiceId, data) => request(`/api/v1/invoices/${invoiceId}/pay`, { method: 'PUT', body: JSON.stringify(data) }),
    getRevenue: () => request('/api/v1/invoices/analytics/revenue-by-plan'),
};

const ClientAPI = {
    search: (query) => request(`/api/v1/clients/search?${new URLSearchParams(query)}`),
    update: (id, d) => request(`/api/v1/clients/${id}`, { method: 'PUT', body: JSON.stringify(d) }),
    delete: (id) => request(`/api/v1/clients/${id}`, { method: 'DELETE' }),
    getHistory: (id) => request(`/api/v1/clients/${id}/history`),
    getActivity: () => request('/api/v1/clients/analytics/activity'),
};

const MembershipAPI = {
    purchase: (data) => request('/api/v1/memberships', { method: 'POST', body: JSON.stringify(data) }),
    getActive: (clientId) => request(`/api/v1/memberships/active/${clientId}`),
};

const EnrollmentAPI = {
    create: (data) => request('/api/v1/enrollments', { method: 'POST', body: JSON.stringify(data) }),
};