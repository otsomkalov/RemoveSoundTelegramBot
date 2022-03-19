using Bot.Models;
using Microsoft.EntityFrameworkCore;

namespace Bot.Data;

public class AppDbContext : DbContext
{
    public DbSet<Conversion> Conversions { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}
