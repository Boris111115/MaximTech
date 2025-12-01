using Task2.Models;
using Task2.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (!context.Drivers.Any())
        {
            context.Drivers.AddRange(
                new Driver { Id = 1, X = 10, Y = 10, IsAvailable = false },
                new Driver { Id = 2, X = 20, Y = 20, IsAvailable = false },
                new Driver { Id = 3, X = 30, Y = 30, IsAvailable = false },
                new Driver { Id = 4, X = 40, Y = 40, IsAvailable = false } 
            );
            context.SaveChanges();
        }
    }
}