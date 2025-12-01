using Task2.Models;
public interface IDriverService
{
    Task<bool> UpdateDriverCoordinatesAsync(UpdateDriverRequest request);
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    bool IsValidCoordinates(int x, int y);
    bool IsCoordinateOccupied(int x, int y, int? excludeDriverId = null);
    Task<bool> CheckDriverExistsAsync(int driverId);
}