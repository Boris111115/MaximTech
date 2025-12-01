using Microsoft.EntityFrameworkCore;
using Task2.Data;     
using Task2.Services;    
using Task2.Models;
using Task2.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("DriversDb"));

builder.Services.Configure<Settings>(
    builder.Configuration.GetSection("Settings"));
builder.Services.Configure<MapSettings>(
    builder.Configuration.GetSection("MapSettings"));

builder.Services.AddScoped<IDriverFinder, SimpleSortFinder>();
builder.Services.AddScoped<IDriverService, DriverService>();

builder.Services.AddSingleton<IRequestLimiter, RequestLimiter>();

var settings = builder.Configuration.GetSection("Settings").Get<Settings>();
Console.WriteLine($"🚨 ДИАГНОСТИКА: ParallelLimit = {settings?.ParallelLimit ?? -1}");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedData.Initialize(context);
}

app.UseMiddleware<RequestLimitMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/status", (IRequestLimiter limiter) => 
{
    return Results.Ok(new
    {
        currentRequests = limiter.GetCurrentRequests(),
        limit = limiter.GetLimit(),
        status = limiter.GetCurrentRequests() < limiter.GetLimit() ? "Available" : "Busy"
    });
});

app.Run();