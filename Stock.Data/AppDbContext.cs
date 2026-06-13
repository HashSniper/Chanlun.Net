using Stock.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Stock.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<StockInfo> StockInfos { get; set; }
    public DbSet<TdxCurrentKlineView> TdxCurrentKlineViews { get; set; }

    // 账户与交易相关
    public DbSet<TradingAccount> TradingAccounts { get; set; }
    public DbSet<StockPosition> StockPositions { get; set; }
    public DbSet<StockTradeRecord> StockTradeRecords { get; set; }

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
            entity.HasAlternateKey(e => e.Symbol); // Symbol 作为备用键，供 Kline 外键引用
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Exchange).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SettlementType)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired()
                .HasDefaultValue(TradeSettlementType.T1);
            entity.Property(e => e.SyncStatus)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(SyncStatus.NotSynced);
        });

        // 通达信当前K线视图
        modelBuilder.Entity<TdxCurrentKlineView>(entity =>
        {
            entity.ToTable("TdxCurrentKlineView");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Symbol, e.Resolution });
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(e => e.StartTime).HasPrecision(0);
            entity.Property(e => e.EndTime).HasPrecision(0);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
        });

        // 交易账户
        modelBuilder.Entity<TradingAccount>(entity =>
        {
            entity.ToTable("TradingAccount");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalBalance).HasPrecision(18, 4);
            entity.Property(e => e.AvailableBalance).HasPrecision(18, 4);
            entity.Property(e => e.FrozenBalance).HasPrecision(18, 4);
            entity.Property(e => e.MarketValue).HasPrecision(18, 4);
            entity.Property(e => e.TotalProfitLoss).HasPrecision(18, 4);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
        });

        // 股票持仓
        modelBuilder.Entity<StockPosition>(entity =>
        {
            entity.ToTable("StockPosition");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.Symbol }).IsUnique();
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.AvailableQuantity).HasPrecision(18, 4);
            entity.Property(e => e.AverageCost).HasPrecision(18, 4);
            entity.Property(e => e.TotalCost).HasPrecision(18, 4);
            entity.Property(e => e.ProfitLoss).HasPrecision(18, 4);
            entity.Property(e => e.ProfitLossRate).HasPrecision(18, 6);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt).HasPrecision(0);

            entity.HasOne(e => e.Account)
                  .WithMany(a => a.Positions)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 股票交易记录
        modelBuilder.Entity<StockTradeRecord>(entity =>
        {
            entity.ToTable("StockTradeRecord");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.TradeTime });
            entity.HasIndex(e => new { e.Symbol, e.TradeTime });
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Direction).HasConversion<string>().HasMaxLength(10);
            entity.Property(e => e.Price).HasPrecision(18, 4);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.Amount).HasPrecision(18, 4);
            entity.Property(e => e.Fee).HasPrecision(18, 4);
            entity.Property(e => e.Tax).HasPrecision(18, 4);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 4);
            entity.Property(e => e.TradeTime).HasPrecision(0);
            entity.Property(e => e.Remark).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasPrecision(0);

            entity.HasOne(e => e.Account)
                  .WithMany(a => a.TradeRecords)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // K线基类使用 TPC（Table-per-Concrete-Type）策略
        modelBuilder.Entity<KlineBase>(entity =>
        {
            entity.UseTpcMappingStrategy();
            entity.Property(e => e.TradeTime).HasPrecision(0);   // 精确到秒
            entity.Property(e => e.CreatedAt).HasPrecision(0);   // 精确到秒

            // K线 Symbol 关联到 StockInfo.Symbol
            entity.HasOne<StockInfo>()
                .WithMany()
                .HasForeignKey(e => e.Symbol)
                .HasPrincipalKey(e => e.Symbol)
                .OnDelete(DeleteBehavior.Restrict);
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
