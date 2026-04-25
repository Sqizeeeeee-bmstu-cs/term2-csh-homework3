using Microsoft.Extensions.Caching.Memory;

using homework3.Models;
using homework3.Configs;


namespace homework3.Services;

public class StockService
{
    private readonly HttpClient _httpClient;
    private readonly ICustomLogger _logger;
    private readonly ApiConfig _config;
    private readonly IMemoryCache _cache;

    public StockService(HttpClient httpClient, ICustomLogger logger, ApiConfig config, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
        _cache = cache;
    }

    public async Task<StockPrice> GetStockPriceAsync(string symbol)
    {

        string cacheKey = $"quote_{symbol.ToUpper()}";

        if (_cache.TryGetValue(cacheKey, out StockPrice cachedPrice))
        {
            _logger.LogAction($"Данные по {symbol} взяты из кэша", LogLevels.Info);
            return cachedPrice;
        }

        string URL = $"{_config.BaseUrl}/api/v1/quote?symbol={symbol}&token={_config.ApiKey}";
        _logger.LogAction($"запрос на {symbol}", LogLevels.Info);

        try 
        {
            var result = await _httpClient.GetFromJsonAsync<StockPrice>(URL);
            _logger.LogAction($"данные на запрос по {symbol} получены", LogLevels.Info);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogAction($"ошибка по запросу на {symbol}: {ex}", LogLevels.Error);
            return null;
        }
    }
}
