using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Stock.Data;

/// <summary>
/// EF Core 设计时 DbContext 工厂（供 dotnet ef migrations 使用）
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=.;Database=StockDb;User Id=sa;Password=dd461233;MultipleActiveResultSets=true;TrustServerCertificate=True",
            sqlOptions => sqlOptions.CommandTimeout(3600));
        return new AppDbContext(optionsBuilder.Options);
    }
}
