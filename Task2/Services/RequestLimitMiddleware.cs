public class RequestLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRequestLimiter _limiter;
    private readonly ILogger<RequestLimitMiddleware> _logger;

    public RequestLimitMiddleware(
        RequestDelegate next, 
        IRequestLimiter limiter,
        ILogger<RequestLimitMiddleware> logger)
    {
        _next = next;
        _limiter = limiter;
        _logger = logger;
    }

    
    public async Task InvokeAsync(HttpContext context)
    {
        var acquired = await _limiter.TryAcquireSlotAsync(context.RequestAborted);
        
        if (!acquired)
        {
            _logger.LogWarning(
                "Отклонен запрос {Method} {Path}. Достигнут лимит: {Current}/{Limit}",
                context.Request.Method,
                context.Request.Path,
                _limiter.GetCurrentRequests(),
                _limiter.GetLimit());

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Service is busy. Try again later.",
                currentRequests = _limiter.GetCurrentRequests(),
                limit = _limiter.GetLimit()
            });
            
            return;
        }

        try
        {
            _logger.LogDebug(
                "Принят запрос {Method} {Path}. Текущие: {Current}/{Limit}",
                context.Request.Method,
                context.Request.Path,
                _limiter.GetCurrentRequests(),
                _limiter.GetLimit());

            await _next(context);
        }
        finally
        {
            _limiter.ReleaseSlot();
        }
    }
}