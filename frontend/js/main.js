function showPage(name) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.nav-links a, .nav-mobile a').forEach(a => a.classList.remove('active'));

    const page = document.getElementById(`page-${name}`);
    if (page) page.classList.add('active');
    document.querySelectorAll(`[data-page="${name}"]`).forEach(a => a.classList.add('active'));

    closeMobileMenu();
    window.scrollTo({ top: 0, behavior: 'instant' });
    setTimeout(() => initReveal(), 60);

    switch (name) {
        case 'home': initHomePage(); break;
        case 'trainers': loadTrainers(); break;
        case 'tariffs': loadTariffs(); break;
        case 'schedule': loadSchedule(); break;
        case 'invoices': loadInvoices(); break;
    }
}

function toggleMobileMenu() {
    document.getElementById('nav-mobile')?.classList.toggle('open');
    document.getElementById('nav-burger')?.classList.toggle('open');
}
function closeMobileMenu() {
    document.getElementById('nav-mobile')?.classList.remove('open');
    document.getElementById('nav-burger')?.classList.remove('open');
}


function showToast(msg, type = 'info') {
    const c = document.getElementById('toast-container');
    if (!c) return;
    const icons = { info: 'ℹ️', success: '✅', error: '⚠️' };
    const el = document.createElement('div');
    el.className = `toast ${type}`;
    el.innerHTML = `<span>${icons[type] || 'ℹ️'}</span><span>${msg}</span>`;
    c.appendChild(el);
    setTimeout(() => { el.style.opacity = '0'; el.style.transition = 'opacity .3s'; setTimeout(() => el.remove(), 300); }, 3500);
}
function showLoading(id) {
    const el = document.getElementById(id);
    if (el) el.innerHTML = `<div class="loading"><div class="spinner"></div><span>Завантаження...</span></div>`;
}
function showEmpty(id, msg = 'Нічого не знайдено') {
    const el = document.getElementById(id);
    if (el) el.innerHTML = `<div class="empty-state"><div class="empty-icon">📭</div><p>${msg}</p></div>`;
}


function initReveal() {
    const page = document.querySelector('.page.active');
    if (!page) return;

    // знаходимо всі елементи з rv класом
    const els = page.querySelectorAll('.rv');
    if (!els.length) return;

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (entry.isIntersecting) {
                const el = entry.target;
                // затримка через data-delay або inline style
                const delay = el.dataset.delay || el.style.getPropertyValue('--rv-delay') || '0s';
                el.style.setProperty('--rv-delay', delay);
                requestAnimationFrame(() => el.classList.add('in'));
                observer.unobserve(el);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

    els.forEach(el => {
        // скидаємо якщо вже було
        el.classList.remove('in');
        observer.observe(el);
    });
}


function animateCounters() {
    document.querySelectorAll('.stat-card[data-val]').forEach((card, i) => {
        const target = parseInt(card.dataset.val);
        const suffix = card.dataset.suffix || '';
        const el = card.querySelector('.stat-num');
        if (!el || el.dataset.done) return;
        el.dataset.done = '1';

        let start = null;
        const duration = 1600;

        const step = (timestamp) => {
            if (!start) start = timestamp;
            const progress = Math.min((timestamp - start) / duration, 1);
            // easeOutExpo
            const eased = progress === 1 ? 1 : 1 - Math.pow(2, -10 * progress);
            el.textContent = Math.round(eased * target) + suffix;
            if (progress < 1) requestAnimationFrame(step);
        };

        setTimeout(() => requestAnimationFrame(step), i * 180);
    });
}


function initNavScroll() {
    const nav = document.getElementById('nav');
    if (!nav) return;
    const handler = () => nav.classList.toggle('scrolled', window.scrollY > 30);
    window.addEventListener('scroll', handler, { passive: true });
    handler();
}


function initParticles() {
    const container = document.querySelector('.hero-particles');
    if (!container) return;
    container.innerHTML = '';
    for (let i = 0; i < 18; i++) {
        const span = document.createElement('span');
        span.style.cssText = `
      left: ${Math.random() * 100}%;
      bottom: ${Math.random() * 20}%;
      width: ${1 + Math.random() * 2}px;
      height: ${1 + Math.random() * 2}px;
      animation-duration: ${6 + Math.random() * 12}s;
      animation-delay: ${Math.random() * 8}s;
    `;
        container.appendChild(span);
    }
}


function initHomePage() {
    initParticles();
    loadTariffsHome();

    // Запускаємо лічильники при scroll до секції
    const aboutSection = document.getElementById('about');
    if (aboutSection) {
        const obs = new IntersectionObserver((entries) => {
            if (entries[0].isIntersecting) { animateCounters(); obs.disconnect(); }
        }, { threshold: 0.25 });
        obs.observe(aboutSection);
    }
}


const TRAINER_PHOTOS = [
    'images/trainers/bro1.jpg', 'images/trainers/bro2.jpg',
    'images/trainers/women1.jpg', 'images/trainers/women2.jpg',
    'images/trainers/bro3.jpg', 'images/trainers/women3.jpg',
];
const TRAINER_EXP = [
    '8 років · КМС з важкої атлетики',
    '6 років · Сертифікат RYT-200',
    '5 років · Майстер спорту з бігу',
    '7 років · CrossFit Level 2',
    '12 років · Чемпіон України',
    '4 роки · Фізіотерапевт',
];

async function loadTrainers() {
    showLoading('trainers-grid');
    try {
        const data = await CoachAPI.getAll();
        renderTrainers(data);
    } catch {
        showEmpty('trainers-grid', 'Не вдалось завантажити тренерів');
        showToast('Помилка завантаження тренерів', 'error');
    }
}

function renderTrainers(trainers) {
    const grid = document.getElementById('trainers-grid');
    if (!grid) return;
    if (!trainers?.length) { showEmpty('trainers-grid', 'Тренери не знайдені'); return; }

    grid.innerHTML = trainers.map((t, i) => {
        const initials = (t.name || 'TT').split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase();
        const photo = TRAINER_PHOTOS[i] || null;
        const exp = TRAINER_EXP[i] || '';
        const delay = (i * 0.08).toFixed(2) + 's';

        const photoHtml = photo
            ? `<div class="trainer-photo-wrap">
           <img class="trainer-photo" src="${photo}" alt="${t.name || 'Тренер'}"
             onerror="this.parentElement.innerHTML='<div class=\\'trainer-avatar-placeholder\\'>${initials}</div>'">
         </div>`
            : `<div class="trainer-photo-wrap"><div class="trainer-avatar-placeholder">${initials}</div></div>`;

        return `
      <div class="trainer-card" style="animation-delay:${delay}">
        ${photoHtml}
        <div class="trainer-info">
          <h3>${t.name || 'Тренер'}</h3>
          <div class="trainer-spec">${t.specialization || 'Загальна підготовка'}</div>
          <div class="trainer-exp">⏱ ${exp}</div>
          <div class="trainer-email">✉ ${t.email || ''}</div>
        </div>
      </div>`;
    }).join('');

    setTimeout(() => initReveal(), 60);
}


async function loadTariffs() {
    showLoading('tariffs-grid');
    try {
        const data = await PlanAPI.getAll();
        renderTariffs(data, 'tariffs-grid');
    } catch {
        showEmpty('tariffs-grid', 'Не вдалось завантажити тарифи');
        showToast('Помилка завантаження тарифів', 'error');
    }
}

async function loadTariffsHome() {
    const grid = document.getElementById('tariffs-grid-home');
    if (!grid) return;
    showLoading('tariffs-grid-home');
    try {
        const data = await PlanAPI.getAll();
        renderTariffs(data, 'tariffs-grid-home');
    } catch {
        showEmpty('tariffs-grid-home', 'Не вдалось завантажити тарифи');
    }
}

function renderTariffs(plans, gridId) {
    const grid = document.getElementById(gridId);
    if (!grid) return;
    if (!plans?.length) { showEmpty(gridId, 'Тарифи не знайдені'); return; }

    const sorted = [...plans].sort((a, b) => a.price - b.price);
    const featuredId = sorted[Math.floor(sorted.length / 2)]?.planId;

    grid.innerHTML = plans.map((p, i) => {
        const featured = p.planId === featuredId;
        const months = p.durationMonths || 1;
        const label = months === 1 ? 'місяць' : months < 5 ? 'місяці' : 'місяців';
        const delay = (i * 0.1).toFixed(2) + 's';
        return `
      <div class="tariff-card ${featured ? 'featured' : ''}" style="animation-delay:${delay}">
        ${featured ? '<div class="tariff-featured-badge">⭐ Популярний</div>' : ''}
        <div class="tariff-name">${p.name || 'Тариф'}</div>
        <div class="tariff-price"><span class="tariff-currency">₴</span>${Math.round(p.price) || 0}</div>
        <div class="tariff-duration">${months} ${label}</div>
        <div class="tariff-desc">${p.description || 'Необмежений доступ до всіх зон залу'}</div>
        <button class="tariff-btn" onclick="onSelectTariff(${p.planId},'${p.name}')">Обрати план</button>
      </div>`;
    }).join('');
}

function onSelectTariff(id, name) {
    if (!Auth.isLoggedIn()) {
        showToast('Увійдіть, щоб придбати абонемент', 'error');
        showPage('login'); return;
    }
    showToast(`Ви обрали план: ${name}`, 'success');
}


function getTodayDate() { return new Date().toISOString().split('T')[0]; }
function formatTime(ts) {
    if (!ts) return '--:--';
    try { return new Date(ts).toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' }); }
    catch { return '--:--'; }
}

async function loadSchedule(date) {
    const inp = document.getElementById('schedule-date');
    const d = date || inp?.value || getTodayDate();
    if (inp) inp.value = d;
    showLoading('schedule-list');
    try {
        const data = await ClassAPI.getSchedule(d);
        renderSchedule(data);
    } catch {
        showEmpty('schedule-list', 'Немає занять або помилка завантаження');
    }
}

function renderSchedule(classes) {
    const list = document.getElementById('schedule-list');
    if (!list) return;
    if (!classes?.length) { showEmpty('schedule-list', 'Занять на цей день не заплановано'); return; }
    list.innerHTML = classes.map((c, i) => `
    <div class="schedule-item" style="animation-delay:${i * .05}s">
      <div class="sch-time">${formatTime(c.startTime)}</div>
      <div>
        <div class="sch-name">${c.classTypeName || c.name || 'Тренування'}</div>
        <div class="sch-meta">${formatTime(c.startTime)} — ${formatTime(c.endTime)}</div>
      </div>
      <div class="sch-coach">🏋️ ${c.coachName || 'Тренер'}</div>
      <div class="sch-capacity"><strong>${c.capacity || '—'}</strong> місць</div>
    </div>`).join('');
}


async function loadInvoices() {
    const user = Auth.getUser();
    if (!user) {
        document.getElementById('invoices-content').innerHTML = `
      <div class="empty-state"><div class="empty-icon">🔒</div><p>Увійдіть, щоб переглянути рахунки</p></div>
      <div style="text-align:center;margin-top:1.5rem">
        <button class="btn-accent" onclick="showPage('login')">Увійти</button>
      </div>`;
        return;
    }
    showLoading('invoices-content');
    try {
        const data = await InvoiceAPI.getPending(user.id);
        renderInvoices(data);
    } catch {
        showEmpty('invoices-content', 'Не вдалось завантажити рахунки');
        showToast('Помилка завантаження рахунків', 'error');
    }
}

function renderInvoices(invoices) {
    const c = document.getElementById('invoices-content');
    if (!c) return;
    if (!invoices?.length) {
        c.innerHTML = `<div class="empty-state"><div class="empty-icon">🎉</div><p>Немає рахунків</p></div>`;
        return;
    }
    const total = invoices.reduce((s, i) => s + (i.amount || 0), 0);
    const paid = invoices.filter(i => /paid/i.test(i.status)).length;
    const pending = invoices.filter(i => /pending/i.test(i.status)).length;
    const statusMap = {
        paid: ['badge-paid', 'Оплачено'],
        Paid: ['badge-paid', 'Оплачено'],
        pending: ['badge-pending', 'Очікує'],
        Pending: ['badge-pending', 'Очікує'],
        overdue: ['badge-overdue', 'Прострочено'],
        Overdue: ['badge-overdue', 'Прострочено'],
    };
    c.innerHTML = `
    <div class="invoices-summary">
      <div class="summary-card"><div class="s-label">Загальна сума</div><div class="s-val">₴${total.toLocaleString('uk-UA')}</div></div>
      <div class="summary-card"><div class="s-label">Оплачено</div><div class="s-val">${paid}</div></div>
      <div class="summary-card"><div class="s-label">Очікує</div><div class="s-val">${pending}</div></div>
    </div>
    <div class="invoices-table-wrap">
      <table class="invoices-table">
        <thead><tr><th>#</th><th>Дата</th><th>Сума</th><th>Статус</th><th>Дія</th></tr></thead>
        <tbody>
          ${invoices.map(inv => {
        const [badge, text] = statusMap[inv.status] || ['badge-pending', inv.status];
        const isPending = /pending/i.test(inv.status);
        const date = inv.date ? new Date(inv.date).toLocaleDateString('uk-UA') : '—';
        return `<tr>
              <td style="color:var(--gray)">#${inv.invoiceId || inv.id}</td>
              <td>${date}</td>
              <td style="font-weight:600">₴${inv.amount?.toLocaleString('uk-UA') || 0}</td>
              <td><span class="badge ${badge}">${text}</span></td>
              <td>${isPending ? `<button class="pay-btn" onclick="payInvoice(${inv.invoiceId || inv.id})">Оплатити</button>` : '—'}</td>
            </tr>`;
    }).join('')}
        </tbody>
      </table>
    </div>`;
}

async function payInvoice(id) {
    try {
        await InvoiceAPI.pay(id, { paymentMethod: 'Card' });
        showToast('Рахунок успішно оплачено! ✅', 'success');
        loadInvoices();
    } catch { showToast('Помилка оплати', 'error'); }
}


async function handleLogin(e) {
    e.preventDefault();
    const email = document.getElementById('login-email').value.trim();
    const pass = document.getElementById('login-password').value;
    const errorEl = document.getElementById('login-error');
    const btn = document.getElementById('login-submit');
    if (!email || !pass) { showErr(errorEl, 'Заповніть всі поля'); return; }
    btn.disabled = true; btn.textContent = 'Входжу...';
    errorEl?.classList.remove('show');
    try {
        const user = await loginUser(email, pass);
        updateNavAuth();
        showToast(`Ласкаво просимо, ${user.name?.split(' ')[0] || 'друже'}! 💪`, 'success');
        showPage('home');
    } catch (err) {
        showErr(errorEl, err.message || 'Невірний email або пароль');
    } finally {
        btn.disabled = false; btn.textContent = 'Увійти';
    }
}
function showErr(el, msg) { if (el) { el.textContent = msg; el.classList.add('show'); } }


document.addEventListener('DOMContentLoaded', () => {
    const dateInput = document.getElementById('schedule-date');
    if (dateInput) dateInput.value = getTodayDate();

    updateNavAuth();
    initNavScroll();
    showPage('home');

    document.getElementById('login-form')?.addEventListener('submit', handleLogin);
    document.getElementById('register-form')?.addEventListener('submit', handleRegister);
    document.getElementById('schedule-date')?.addEventListener('change', e => loadSchedule(e.target.value));

    // Закрити mobile menu при кліку поза ним
    document.addEventListener('click', e => {
        const menu = document.getElementById('nav-mobile');
        const burger = document.getElementById('nav-burger');
        if (menu?.classList.contains('open') && !menu.contains(e.target) && !burger?.contains(e.target)) {
            closeMobileMenu();
        }
    });
});


function switchAuthTab(tab) {
    document.getElementById('form-login').style.display = tab === 'login' ? 'block' : 'none';
    document.getElementById('form-register').style.display = tab === 'register' ? 'block' : 'none';
    document.getElementById('tab-login').classList.toggle('active', tab === 'login');
    document.getElementById('tab-register').classList.toggle('active', tab === 'register');
    // скидаємо помилки
    ['login-error', 'register-error'].forEach(id => {
        const el = document.getElementById(id);
        if (el) { el.textContent = ''; el.classList.remove('show'); }
    });
}

async function handleRegister(e) {
    e.preventDefault();
    const name = document.getElementById('reg-name').value.trim();
    const email = document.getElementById('reg-email').value.trim();
    const phone = document.getElementById('reg-phone').value.trim();
    const password = document.getElementById('reg-password').value;
    const errorEl = document.getElementById('register-error');
    const btn = document.getElementById('register-submit');

    if (!name || !email || !phone || !password) { showErr(errorEl, 'Заповніть всі поля'); return; }
    if (password.length < 4) { showErr(errorEl, 'Пароль мінімум 4 символи'); return; }

    btn.disabled = true; btn.textContent = 'Створюю акаунт...';
    errorEl?.classList.remove('show');
    try {
        const user = await registerUser(name, email, password, phone);
        updateNavAuth();
        showToast(`Акаунт створено! Ласкаво просимо, ${user.name?.split(' ')[0]}! 💪`, 'success');
        showPage('home');
    } catch (err) {
        showErr(errorEl, err.message || 'Помилка реєстрації');
    } finally {
        btn.disabled = false; btn.textContent = 'Створити акаунт';
    }
}