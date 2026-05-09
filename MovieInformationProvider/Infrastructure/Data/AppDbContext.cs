using Microsoft.EntityFrameworkCore;
using MovieInformationProvider.Domain.Entities;

namespace MovieInformationProvider.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SearchStatistic> SearchStatistics => Set<SearchStatistic>();
}