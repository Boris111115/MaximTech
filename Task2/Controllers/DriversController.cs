using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Task2.Models; 
using Task2.Services;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;
    private readonly ILogger<DriversController> _logger;

    public DriversController(IDriverService driverService, ILogger<DriversController> logger)
    {
        _driverService = driverService;
        _logger = logger;
    }

    [HttpPut("coordinates")]
    public async Task<IActionResult> UpdateDriverCoordinates([FromBody] UpdateDriverRequest request)
    {
        if (!_driverService.IsValidCoordinates(request.X, request.Y))
        {
            _logger.LogWarning("Некорректные координаты: ({X}, {Y})", request.X, request.Y);
            return BadRequest(new { error = "Координаты некорректны" });
        }

        if (_driverService.IsCoordinateOccupied(request.X, request.Y, request.Id))
        {
            _logger.LogWarning("Координаты ({X}, {Y}) уже заняты", request.X, request.Y);
            return BadRequest(new { error = "Здесь уже находится другой водитель" });
        }

        bool driverExistsBefore = await _driverService.CheckDriverExistsAsync(request.Id);
        _logger.LogInformation("Водитель {DriverId} существует до операции: {Exists}", 
            request.Id, driverExistsBefore);

        var result = await _driverService.UpdateDriverCoordinatesAsync(request);
        
        if (!result)
        {
            _logger.LogWarning("Не удалось обновить координаты водителя {DriverId}", request.Id);
            return BadRequest(new { error = "Не удалось обновить координаты" });
        }
        string message = driverExistsBefore 
            ? "Координаты успешно изменены" 
            : "Координаты успешно добавлены";
        
        _logger.LogInformation("Операция завершена: {Message} для водителя {DriverId}", 
            message, request.Id);
        
        return Ok(new { message });
    }
}