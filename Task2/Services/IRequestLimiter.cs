public interface IRequestLimiter
{
    Task<bool> TryAcquireSlotAsync(CancellationToken cancellationToken = default);
    void ReleaseSlot();
    int GetCurrentRequests();
    int GetLimit();
}