using Microsoft.EntityFrameworkCore;
using MovieInformationProvider.Data.Common;

namespace MovieInformationProvider.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SearchStatistic> SearchStatistics => Set<SearchStatistic>();
}