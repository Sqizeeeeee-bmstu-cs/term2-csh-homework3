
async function searchStock() {
    const input = document.getElementById('tickerInput');
    const ticker = input.value.toUpperCase().trim();
    const resultContainer = document.getElementById('stockResult');

    if (!ticker) return;

    try {
        const response = await fetch(`/api/stocks/${ticker}`);
        
        if (!response.ok) {
            const errorData = await response.json();
            alert(errorData.error || "Акция не найдена");
            return;
        }

        const data = await response.json();
        
        renderStockCard(ticker, data, resultContainer);
        
    } catch (error) {
        console.error("Ошибка:", error);
    }
}

function renderStockCard(symbol, data, container) {
    const colorClass = data.dp >= 0 ? 'positive' : 'negative';
    const sign = data.dp >= 0 ? '+' : '';

    container.innerHTML = `
        <div class="stock-card">
            <h3>${symbol} <span>${sign}${data.dp.toFixed(2)}%</span></h3>
            <p class="price">${data.c.toFixed(2)} $</p>
            <p class="change ${colorClass}">
                Изменение: ${sign}${data.d.toFixed(2)} $
            </p>
            <button onclick="buyStock('${symbol}', 1)" style="margin-top: 15px; width: 100%;">
                Купить 1 шт.
            </button>
        </div>
    `;
}

async function buyStock(ticker, quantity) {
    try {
        const response = await fetch('/api/stocks/buy', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ticker, quantity })
        });

        if (response.ok) {

            loadPortfolio();
        }
    } catch (error) {
        console.error("Ошибка при покупке:", error);
    }
}

async function loadPortfolio() {
    const portfolioContainer = document.getElementById('portfolioList');
    
    try {
        const response = await fetch('/api/stocks/portfolio');
        const items = await response.json();

        portfolioContainer.innerHTML = items.map(item => `
            <div class="stock-card" style="border-left-color: var(--text-dim)">
                <h3>${item.ticker}</h3>
                <p class="price">${item.buyPrice.toFixed(2)} $</p>
                <p class="change">Кол-во: ${item.quantity} шт.</p>
                <small style="color: var(--text-dim)">Куплено: ${new Date(item.purchaseDate).toLocaleDateString()}</small>
            </div>
        `).join('');
    } catch (error) {
        console.error("Ошибка загрузки портфеля:", error);
    }
}

loadPortfolio();
