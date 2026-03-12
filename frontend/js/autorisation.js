const Auth = {

    getUser() {
        try { return JSON.parse(localStorage.getItem('gym_user')); }
        catch { return null; }
    },

    getToken() {
        return localStorage.getItem('gym_token');
    },

    isLoggedIn() {
        return !!this.getToken();
    },

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
    const res = await request('/api/v1/clients/search?email=' + encodeURIComponent(email));

    if (res && res.length > 0) {
        const user = res[0];
        Auth.setSession(`demo-token-${user.clientId}`, {
            id: user.clientId,
            name: user.name,
            email: user.email,
        });
        return user;
    }
    throw new Error('Користувача не знайдено');
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