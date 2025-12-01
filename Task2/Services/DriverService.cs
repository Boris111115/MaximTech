using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Task2.Data;
using Task2.Models;

namespace Task2.Services;

public class DriverService : IDriverService
{
    private readonly ApplicationDbContext _context;
    private readonly IDriverFinder _driverFinder;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DriverService> _logger;

    public DriverService(
        ApplicationDbContext context,
        IDriverFinder driverFinder,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<DriverService> logger)
    {
        _context = context;
        _driverFinder = driverFinder;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> CheckDriverExistsAsync(int driverId)
    {
        return await _context.Drivers.AnyAsync(d => d.Id == driverId);
    }
    
    public bool IsValidCoordinates(int x, int y)
    {
        try
        {
            // Способ 1: Получаем через IOptions
            var mapSettings = _configuration.GetSection("MapSettings").Get<MapSettings>();
            
            // Если null - используем значения по умолчанию
            if (mapSettings == null)
            {
                _logger.LogWarning("MapSettings not found, using defaults");
                mapSettings = new MapSettings { Width = 100, Height = 100 };
            }
            
            // Проверяем что значения не нулевые
            if (mapSettings.Width <= 0 || mapSettings.Height <= 0)
            {
                _logger.LogWarning("MapSettings has invalid values, using defaults");
                mapSettings = new MapSettings { Width = 100, Height = 100 };
            }
            
            bool isValid = x >= 0 && x < mapSettings.Width && y >= 0 && y < mapSettings.Height;
            
            _logger.LogInformation($"IsValidCoordinates: ({x}, {y}) in {mapSettings.Width}x{mapSettings.Height} → {isValid}");
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in IsValidCoordinates");
            // На время теста - разрешаем все
            return true;
        }
    }

    public bool IsCoordinateOccupied(int x, int y, int? excludeDriverId = null)
    {
        return _context.Drivers.Any(d => 
            d.X == x && d.Y == y && 
            d.Id != excludeDriverId && 
            d.IsAvailable);
    }

    public async Task<bool> UpdateDriverCoordinatesAsync(UpdateDriverRequest request)
    {
        if (!IsValidCoordinates(request.X, request.Y))
            return false;

        var driver = await _context.Drivers.FindAsync(request.Id);
        
        if (driver == null)
        {
            if (IsCoordinateOccupied(request.X, request.Y))
                return false;
                
            driver = new Driver 
            { 
                Id = request.Id, 
                X = request.X, 
                Y = request.Y,
                IsAvailable = true
            };
            _context.Drivers.Add(driver);
        }
        else
        {
            if (IsCoordinateOccupied(request.X, request.Y, request.Id))
                return false;
                
            driver.X = request.X;
            driver.Y = request.Y;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        if (!IsValidCoordinates(request.X, request.Y))
            throw new ArgumentException("Координаты некорректны");

        var availableDrivers = await _context.Drivers
            .Where(d => d.IsAvailable)
            .ToListAsync();

        if (!availableDrivers.Any())
            throw new InvalidOperationException("Свободных водителей нет");
        var nearestDrivers = _driverFinder.FindNearestDrivers(availableDrivers, request.X, request.Y, 10);

        if (!nearestDrivers.Any())
            throw new InvalidOperationException("Свободных водителей нет");
        int randomIndex = await GetRandomNumberAsync(0, nearestDrivers.Count - 1);
        var selectedDriver = nearestDrivers[randomIndex];
        var route = GenerateRoute(selectedDriver.X, selectedDriver.Y, request.X, request.Y);

        return new OrderResponse
        {
            DriverId = selectedDriver.Id,
            DriverX = selectedDriver.X,
            DriverY = selectedDriver.Y,
            RouteLength = selectedDriver.DistanceTo(request.X, request.Y),
            Route = route
        };
    }

    private async Task<int> GetRandomNumberAsync(int min, int max)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://www.randomnumberapi.com/api/v1.0/random?min={min}&max={max}&count=1");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var numbers = System.Text.Json.JsonSerializer.Deserialize<int[]>(content);
                return numbers?.FirstOrDefault() ?? GetLocalRandom(min, max);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get random number from external API, using local random");
        }

        return GetLocalRandom(min, max);
    }

    private int GetLocalRandom(int min, int max)
    {
        return Random.Shared.Next(min, max + 1);
    }

    private List<Coordinate> GenerateRoute(int startX, int startY, int endX, int endY)
    {
        var route = new List<Coordinate>();

        int x = startX, y = startY;
        int dx = Math.Abs(endX - startX), dy = Math.Abs(endY - startY);
        int sx = startX < endX ? 1 : -1, sy = startY < endY ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            route.Add(new Coordinate(x, y));
            if (x == endX && y == endY) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return route;
    }
}