using Microsoft.EntityFrameworkCore;
using Task2.Models;

namespace Task2.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Driver> Drivers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>().HasKey(d => d.Id);
    }
}