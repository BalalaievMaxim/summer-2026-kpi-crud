const Auth = {
    getUser() { try { return JSON.parse(localStorage.getItem('gym_user')); } catch { return null; } },
    getToken() { return localStorage.getItem('gym_token'); },
    isLoggedIn() { return !!this.getToken(); },

    setSession(token, user) {
        localStorage.setItem('gym_token', token);
        localStorage.setItem('gym_user', JSON.stringify(user));
    },

    logout() {
        localStorage.removeItem('gym_token');
        localStorage.removeItem('gym_user');
        updateNavAuth();
        showPage('home');
        showToast('Ви вийшли з системи', 'info');
    },
};


async function loginUser(email, password) {
    const res = await request('/api/v1/clients/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
    });
    if (!res) throw new Error('Помилка сервера');
    Auth.setSession(`token-${res.clientId}`, {
        id: res.clientId, name: res.name, email: res.email, phone: res.phone,
    });
    return res;
}


async function registerUser(name, email, password, phone) {
    const res = await request('/api/v1/clients/register', {
        method: 'POST',
        body: JSON.stringify({ name, email, password, phone }),
    });
    if (!res) throw new Error('Помилка сервера');
    Auth.setSession(`token-${res.clientId}`, {
        id: res.clientId, name: res.name, email: res.email, phone: res.phone,
    });
    return res;
}

function updateNavAuth() {
    const user = Auth.getUser();
    const navAuth = document.getElementById('nav-auth');
    if (!navAuth) return;

    if (user) {
        navAuth.innerHTML = `
      <span class="nav-user">Привіт, <strong>${user.name?.split(' ')[0] || 'Клієнт'}</strong></span>
      <button class="btn-ghost" onclick="Auth.logout()">Вийти</button>
    `;
    } else {
        navAuth.innerHTML = `
      <button class="btn-accent" onclick="showPage('login')">Увійти</button>
    `;
    }
}