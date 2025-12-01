using Microsoft.Extensions.Options;
using Task2.Models;

public class RequestLimiter : IRequestLimiter, IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<RequestLimiter> _logger;
    private readonly int _limit;
    private int _currentRequests;

    public RequestLimiter(IOptions<Settings> settings, ILogger<RequestLimiter> logger)
    {
        _limit = settings.Value.ParallelLimit;
        _semaphore = new SemaphoreSlim(_limit, _limit);
        _logger = logger;
        _currentRequests = 0;
    }

    public async Task<bool> TryAcquireSlotAsync(CancellationToken cancellationToken = default)
    {
        var acquired = await _semaphore.WaitAsync(0, cancellationToken);
        
        if (acquired)
        {
            Interlocked.Increment(ref _currentRequests);
            _logger.LogDebug("Запрос принят. Текущие запросы: {Current}/{Limit}", 
                _currentRequests, _limit);
        }
        else
        {
            _logger.LogWarning("Достигнут лимит параллельных запросов: {Current}/{Limit}", 
                _currentRequests, _limit);
        }
        
        return acquired;
    }

    public void ReleaseSlot()
    {
        _semaphore.Release();
        Interlocked.Decrement(ref _currentRequests);
        _logger.LogDebug("Запрос завершен. Текущие запросы: {Current}/{Limit}", 
            _currentRequests, _limit);
    }

    public int GetCurrentRequests() => _currentRequests;
    public int GetLimit() => _limit;

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}