
let modalUpdateInterval = null;
let currentOpenTicker = null;

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
    loadUserProfile();
}

function showAppTab(tabName) {
    document.querySelectorAll('.tab-pane').forEach(pane => pane.classList.add('hidden'));
    document.querySelectorAll('.nav-btn').forEach(btn => btn.classList.remove('active'));

    const targetPane = document.getElementById(`pane-${tabName}`);
    if (targetPane) targetPane.classList.remove('hidden');
    
    if (event && event.currentTarget) {
        event.currentTarget.classList.add('active');
    }

    if (tabName === 'market') loadTrends();
    if (tabName === 'wallet') {
        loadUserProfile();
        loadTransactions();
        loadPortfolio();
    }
}


async function loadUserProfile() {
    try {
        const response = await fetch('/api/user/profile', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
        });
        if (response.ok) {
            const data = await response.json();
            document.getElementById('user-balance').innerText = Number(data.balance).toFixed(2);
            document.getElementById('user-commissions').innerText = Number(data.totalCommission).toFixed(2);
        }
    } catch (error) {
        console.error("Ошибка загрузки профиля:", error);
    }
}

async function handleTopUp() {
    const amountInput = document.getElementById('topup-amount');
    const amount = parseFloat(amountInput.value);
    if (!amount || amount <= 0) return alert("Введите корректную сумму");

    try {
        const response = await fetch('/api/user/topup', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify({ amount: amount })
        });
        if (response.ok) {
            const data = await response.json();
            alert(data.message);
            document.getElementById('user-balance').innerText = Number(data.newBalance).toFixed(2);
            amountInput.value = '';
            loadTransactions();
        }
    } catch (error) {
        alert("Ошибка при пополнении");
    }
}

async function loadTransactions() {
    const historyContainer = document.getElementById('transaction-history');
    if (!historyContainer) return;

    try {
        const response = await fetch('/api/user/transactions', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
        });
        if (response.ok) {
            const transactions = await response.json();
            if (transactions.length === 0) {
                historyContainer.innerHTML = '<p class="empty-msg">История операций пуста</p>';
                return;
            }
            historyContainer.innerHTML = transactions.map(t => {
                const isPositive = t.type === "TopUp" || t.type === "Sell";
                const colorClass = isPositive ? 'positive' : 'negative';
                const date = new Date(t.createdAt).toLocaleString();
                return `
                    <div class="transaction-item">
                        <div class="transaction-info">
                            <strong>${t.type} ${t.ticker || ''}</strong>
                            <small>${date}</small>
                        </div>
                        <div class="transaction-amount ${colorClass}">
                            ${t.amount > 0 ? '+' : ''}${Number(t.amount).toFixed(2)} $
                        </div>
                    </div>
                `;
            }).join('');
        }
    } catch (e) { console.error(e); }
}


async function loadTrends() {
    const grid = document.getElementById('trends-grid');
    const statusText = document.getElementById('update-status');
    if (!grid) return;

    try {
        const response = await fetch('/api/stocks/trends');
        const data = await response.json();
        
        const time = data.lastUpdated || data.LastUpdated;
        if (statusText && time) statusText.innerText = `Данные актуальны на: ${time}`;

        const stocks = data.stocks || data.Stocks;
        grid.innerHTML = stocks.map(s => {
            const symbol = s.symbol || s.Symbol;
            const price = s.price || s.Price;
            const change = s.change !== undefined ? s.change : s.Change;
            const colorClass = change >= 0 ? 'positive' : 'negative';
            
            return `
                <div class="trend-card">
                    <h4>${symbol}</h4>
                    <div class="trend-price">${Number(price).toFixed(2)} $</div>
                    <div class="trend-change ${colorClass}">${change >= 0 ? '+' : ''}${Number(change).toFixed(2)}%</div>
                    <div class="buy-actions">
                        <input type="number" id="qty-${symbol}" value="1" min="1" class="buy-input-mini">
                        <button onclick="buyStock('${symbol}')" class="btn-buy">Купить</button>
                    </div>
                </div>
            `;
        }).join('');
    } catch (e) { console.error(e); }
}

async function buyStock(symbol) {
    const quantity = parseInt(document.getElementById(`qty-${symbol}`).value);
    if (!quantity || quantity <= 0) return alert("Введите количество");

    try {
        const response = await fetch('/api/stocks/buy', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify({ ticker: symbol, quantity: quantity })
        });
        const data = await response.json();
        if (response.ok) {
            alert(`Куплено ${quantity} шт. ${symbol}`);
            loadUserProfile();
        } else {
            alert(data.error || data.message || "Ошибка покупки");
        }
    } catch (e) { alert("Ошибка сервера"); }
}


async function loadPortfolio() {
    const list = document.getElementById('portfolio-list');
    if (!list) return;

    try {
        const response = await fetch('/api/user/portfolio-stats', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
        });
        const assets = await response.json();

        if (assets.length === 0) {
            list.innerHTML = '<p class="empty-msg">У вас пока нет акций</p>';
            return;
        }

        list.innerHTML = assets.map(a => `
            <div class="asset-card-mini" onclick='showAssetDetails(${JSON.stringify(a)})' 
                 style="background: var(--input-bg); padding: 15px; border-radius: 10px; margin-bottom: 10px; cursor: pointer; border: 1px solid transparent; transition: 0.2s;">
                <div style="display: flex; justify-content: space-between; align-items: center;">
                    <strong>${a.ticker || a.Ticker}</strong>
                    <span style="color: var(--accent)">${a.totalQuantity || a.TotalQuantity} шт.</span>
                </div>
                <div style="font-size: 0.85rem; color: var(--text-dim); margin-top: 5px;">
                    Средняя цена: ${(a.averagePrice || a.AveragePrice).toFixed(2)}$
                </div>
            </div>
        `).join('');

    } catch (e) {
        console.error("Ошибка портфеля:", e);
        list.innerHTML = '<p>Не удалось загрузить активы</p>';
    }
}

async function showAssetDetails(asset) {
    const ticker = asset.ticker || asset.Ticker;

    // ЛОГИКА TOGGLE: Если нажали на ту же акцию, что уже открыта — закрываем её
    if (currentOpenTicker === ticker) {
        closeModal();
        return;
    }

    // Запоминаем текущий открытый тикер
    currentOpenTicker = ticker;

    const modal = document.getElementById('asset-modal');
    const details = document.getElementById('modal-details');
    
    const avgPrice = Number(asset.averagePrice || asset.AveragePrice || 0);
    const totalQty = Number(asset.totalQuantity || asset.TotalQuantity || 0);

    // 1. Отрисовка каркаса
    details.innerHTML = `
        <h2 style="margin-bottom: 5px;">${ticker}</h2>
        <p style="color: var(--text-dim); font-size: 0.85rem; margin-bottom: 20px;">Детальная аналитика активов</p>

        <div class="stats-grid-modal">
            <div>Количество <strong>${totalQty} шт.</strong></div>
            <div>Средняя цена <strong>${avgPrice.toFixed(2)} $</strong></div>
            <div id="modal-current-price">Текущая цена <span class="loader-mini">...</span></div>
            <div id="modal-profit">Профит <span class="loader-mini">...</span></div>
        </div>
        
        <div id="modal-history-box">
            <div class="loader">Загрузка данных...</div>
        </div>
    `;
    modal.classList.remove('hidden');

    const updatePriceData = async () => {
        try {
            const response = await fetch(`/api/stocks/${ticker}`);
            const priceData = await response.json();
            
            const currentPrice = priceData.currentPrice || priceData.CurrentPrice || priceData.c || 0;
            const profit = (currentPrice - avgPrice) * totalQty;
            const profitPercent = avgPrice > 0 ? ((currentPrice - avgPrice) / avgPrice) * 100 : 0;
            const profitClass = profit >= 0 ? 'positive' : 'negative';

            const priceEl = document.getElementById('modal-current-price');
            const profitEl = document.getElementById('modal-profit');
            
            if (priceEl) priceEl.innerHTML = `Текущая: <strong>${currentPrice.toFixed(2)}$</strong>`;
            if (profitEl) {
                profitEl.innerHTML = `Профит: <strong>${profit.toFixed(2)}$ (${profitPercent.toFixed(2)}%)</strong>`;
                profitEl.className = profitClass;
            }
            return currentPrice;
        } catch (e) {
            console.error("Ошибка автообновления:", e);
            return null;
        }
    };

    await updatePriceData();

    const history = asset.history || asset.History || [];
    const historyHtml = history.map(h => `
        <div style="display:flex; justify-content:space-between; font-size:0.8rem; margin-bottom:5px; border-bottom:1px solid #334155; padding-bottom:3px;">
            <span>${new Date(h.purchaseDate || h.PurchaseDate).toLocaleDateString()}</span>
            <span>${h.quantity || h.Quantity} шт. по ${(h.buyPrice || h.BuyPrice).toFixed(2)}$</span>
        </div>
    `).join('');
    
    const historyBox = document.getElementById('modal-history-box');
    if (historyBox) {
        historyBox.innerHTML = `
            <p style="margin-bottom:10px; font-size:0.9rem; color:var(--text-dim);">История сделок:</p>
            <div style="background:rgba(0,0,0,0.2); padding:10px; border-radius:8px; max-height: 120px; overflow-y: auto;">
                ${historyHtml || 'История пуста'}
            </div>
            <div class="modal-sell-box" style="display:flex; gap:10px; margin-top:20px;">
                <input type="number" id="modal-sell-qty" value="${totalQty}" min="1" max="${totalQty}" class="buy-input" style="width:80px;">
                <button onclick="executeSale('${ticker}', ${totalQty})" class="btn-logout" style="background:var(--danger); color:white; flex:1;">Продать активы</button>
            </div>
        `;
    }

    if (modalUpdateInterval) clearInterval(modalUpdateInterval);
    modalUpdateInterval = setInterval(updatePriceData, 15000);
}

function closeModal() {
    document.getElementById('asset-modal').classList.add('hidden');
    

    currentOpenTicker = null;
    
    if (modalUpdateInterval) {
        clearInterval(modalUpdateInterval);
        modalUpdateInterval = null;
    }
}

async function executeSale(ticker, maxQty) {
    const quantity = parseInt(document.getElementById('modal-sell-qty').value);
    if (!quantity || quantity <= 0 || quantity > maxQty) return alert("Некорректное количество");

    if (!confirm(`Продать ${quantity} шт. ${ticker}? Комиссия: 1.00$`)) return;

    try {
        const response = await fetch('/api/stocks/sell-grouped', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}` 
            },
            body: JSON.stringify({ ticker: ticker, quantity: quantity })
        });
        if (response.ok) {
            alert("Продано успешно!");
            closeModal();
            loadUserProfile();
            loadPortfolio();
            loadTransactions();
        }
    } catch (e) { console.error(e); }
}

function logout() {
    localStorage.clear();
    location.reload();
}

window.onload = () => {
    if (localStorage.getItem('token')) showApp();
};
