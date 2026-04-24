function switchTab(type) {
    const loginForm = document.getElementById('form-login');
    const registerForm = document.getElementById('form-register');
    const loginTab = document.getElementById('tab-login');
    const registerTab = document.getElementById('tab-register');

    if (type === 'login') {
        loginForm.classList.remove('hidden');
        registerForm.classList.add('hidden');
        loginTab.classList.add('active');
        registerTab.classList.remove('active');
    } else {
        loginForm.classList.add('hidden');
        registerForm.classList.remove('hidden');
        loginTab.classList.remove('active');
        registerTab.classList.add('active');
    }
}

async function onRegister(event) {
    event.preventDefault();
    const username = document.getElementById('reg-name').value;
    const email = document.getElementById('reg-email').value;
    const password = document.getElementById('reg-password').value;

    try {
        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password })
        });
        const data = await response.json();
        if (response.ok) {
            alert('Регистрация успешна! Теперь войдите.');
            switchTab('login');
        } else {
            alert('Ошибка: ' + (data.error || data.message));
        }
    } catch (error) {
        alert('Сервер недоступен');
    }
}

async function onLogin(event) {
    event.preventDefault();
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;

    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        const data = await response.json();
        if (response.ok) {
            localStorage.setItem('token', data.token);
            localStorage.setItem('username', data.username);
            showApp(); 
        } else {
            alert('Неверный логин или пароль');
        }
    } catch (error) {
        alert('Ошибка сервера');
    }
}

function showApp() {
    document.getElementById('auth-screen').classList.add('hidden');
    document.getElementById('main-screen').classList.remove('hidden');
    document.getElementById('user-name-display').innerText = localStorage.getItem('username');
    loadTrends();
}

function logout() {
    localStorage.clear();
    location.reload();
}

window.onload = () => {
    if (localStorage.getItem('token')) {
        showApp();
    }
};

async function loadTrends() {
    const grid = document.getElementById('trends-grid');
    if (!grid) return;

    try {
        const response = await fetch('/api/stocks/trends');
        const stocks = await response.json();
        
        // Поможет нам понять структуру данных в консоли браузера
        console.log("Данные трендов:", stocks);

        grid.innerHTML = stocks.map(s => {
            // Подстраховка под разный регистр букв от .NET
            const symbol = s.symbol || s.Symbol || "???";
            const price = s.price || s.Price || 0;
            const change = s.change !== undefined ? s.change : (s.Change || 0);

            const colorClass = change >= 0 ? 'positive' : 'negative';
            const sign = change >= 0 ? '+' : '';
            
            return `
                <div class="trend-card">
                    <h4>${symbol}</h4>
                    <div class="trend-price">${Number(price).toFixed(2)} $</div>
                    <div class="trend-change ${colorClass}">
                        ${sign}${Number(change).toFixed(2)}%
                    </div>
                </div>
            `;
        }).join('');

    } catch (error) {
        console.error("Ошибка JS при отрисовке:", error);
        grid.innerHTML = '<p>Не удалось загрузить котировки</p>';
    }
}
