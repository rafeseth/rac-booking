using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RacBooking.Infrastructure.Persistence;

/// <summary>Design-time factory so <c>dotnet ef</c> does not require a running API or tenant context.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=racbooking_ef_design;Username=postgres;Password=postgres;");
        return new AppDbContext(optionsBuilder.Options);
    }
}
