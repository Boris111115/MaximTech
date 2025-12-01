
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Task2.Models;

namespace Task2.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IDriverService _driverService;

    public OrdersController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var result = await _driverService.CreateOrderAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex) when (ex.Message == "Координаты некорректны")
        {
            return BadRequest(new { error = "Координаты некорректны" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "Свободных водителей нет")
        {
            return BadRequest(new { error = "Свободных водителей нет" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}