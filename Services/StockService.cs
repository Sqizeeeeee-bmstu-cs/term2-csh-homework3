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

        string URL = $"{_config.BaseUrl}/api/v1/quote?symbol={symbol}&token={_config.ApiKey}";
        _logger.LogAction($"запрос на {symbol}", LogLevels.Info);

        try 
        {
            var result = await _httpClient.GetFromJsonAsync<StockPrice>(URL);
            _logger.LogAction($"данные на запрос по {symbol} получены", LogLevels.Info);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogAction($"ошибка по запросу на {symbol}: {ex}", LogLevels.Error);
            return null;
        }
    }
}
