using System.Net.Http.Json;
using homework3.Models;
using homework3.Configs;

namespace homework3.Services;

public class StockService
{
    private readonly HttpClient _httpClient;
    private readonly ICustomLogger _logger;
    private readonly ApiConfig _config;

    public StockService(HttpClient httpClient, ICustomLogger logger, ApiConfig config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    public async Task<StockPrice?> GetStockPriceAsync(string symbol)
    {
        // 1. Сформируй URL, используя _config.BaseUrl, symbol и _config.ApiKey
        // Подсказка: путь для Finnhub — "/api/v1/quote?symbol={symbol}&token={key}"
        
        // 2. Сделай LogAction, что запрос начат (уровень Info)

        try 
        {
            // 3. Используй _httpClient.GetFromJsonAsync<StockPrice>(url)
            // 4. Верни результат
            return null; // замени на логику
        }
        catch (Exception ex)
        {
            // 5. Если ошибка — залогируй её (уровень Error) и верни null
            return null;
        }
    }
}
