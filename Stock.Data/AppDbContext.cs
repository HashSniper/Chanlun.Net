using Stock.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Stock.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<StockInfo> StockInfos { get; set; }
    public DbSet<UserWatchRecord> UserWatchRecords { get; set; }

    // 分周期K线表（TPC：每个具体类独立一张完整表）
    public DbSet<Kline1m> Kline1m { get; set; }
    public DbSet<Kline5m> Kline5m { get; set; }
    public DbSet<Kline15m> Kline15m { get; set; }
    public DbSet<Kline30m> Kline30m { get; set; }
    public DbSet<Kline60m> Kline60m { get; set; }
    public DbSet<Kline1d> Kline1d { get; set; }
    public DbSet<Kline1w> Kline1w { get; set; }
    public DbSet<Kline1mo> Kline1mo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // StockInfo 配置
        modelBuilder.Entity<StockInfo>(entity =>
        {
            entity.ToTable("StockInfo");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Symbol).IsUnique();
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Exchange).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
        });

        // 用户查看记录
        modelBuilder.Entity<UserWatchRecord>(entity =>
        {
            entity.ToTable("UserWatchRecord");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Symbol, e.Resolution });
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Resolution).HasMaxLength(10).IsRequired();
            entity.Property(e => e.StartTime).HasPrecision(0);
            entity.Property(e => e.EndTime).HasPrecision(0);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
        });

        // K线基类使用 TPC（Table-per-Concrete-Type）策略
        modelBuilder.Entity<KlineBase>(entity =>
        {
            entity.UseTpcMappingStrategy();
            entity.Property(e => e.TradeTime).HasPrecision(0);   // 精确到秒
            entity.Property(e => e.CreatedAt).HasPrecision(0);   // 精确到秒
        });

        // 分钟级K线：每张表独立自增 ID
        modelBuilder.Entity<Kline1m>(entity =>
        {
            entity.ToTable("Kline_1m");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline1m_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Kline5m>(entity =>
        {
            entity.ToTable("Kline_5m");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline5m_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Kline15m>(entity =>
        {
            entity.ToTable("Kline_15m");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline15m_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Kline30m>(entity =>
        {
            entity.ToTable("Kline_30m");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline30m_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Kline60m>(entity =>
        {
            entity.ToTable("Kline_60m");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline60m_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });

        // 日/周/月线：每张表独立自增 ID
        modelBuilder.Entity<Kline1d>(entity =>
        {
            entity.ToTable("Kline_1d");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline1d_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Kline1w>(entity =>
        {
            entity.ToTable("Kline_1w");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline1w_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Kline1mo>(entity =>
        {
            entity.ToTable("Kline_1mo");
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.HasIndex(e => new { e.Symbol, e.TradeTime }).IsUnique().HasDatabaseName("IX_Kline1mo_Symbol_Time");
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
        });
    }
}
