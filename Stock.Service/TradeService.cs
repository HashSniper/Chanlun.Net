using Microsoft.EntityFrameworkCore;
using Stock.Data;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Stock.Service;

/// <summary>
/// 交易服务 —— 处理买入/卖出成交、持仓更新、余额扣减、T+1结算
/// </summary>
public class TradeService : ITradeService
{
    private readonly AppDbContext _dbContext;

    public TradeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockTradeRecord> ExecuteTradeAsync(ExecuteTradeRequest request, CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. 获取账户
            var account = await _dbContext.TradingAccounts
                .FirstOrDefaultAsync(a => a.Id == request.AccountId, ct)
                ?? throw new InvalidOperationException($"账户不存在: {request.AccountId}");

            // 2. 计算成交金额
            var tradeAmount = request.Price * request.Quantity;

            StockTradeRecord record;

            if (request.Direction == TradeDirection.Buy)
            {
                record = await ExecuteBuyAsync(account, request, tradeAmount, ct);
            }
            else
            {
                record = await ExecuteSellAsync(account, request, tradeAmount, ct);
            }

            account.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return record;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// T+1 结算：将昨日买入的不可用持仓变为可用
    /// </summary>
    public async Task SettlementAsync(long accountId, CancellationToken ct = default)
    {
        var positions = await _dbContext.StockPositions
            .Where(p => p.AccountId == accountId && p.AvailableQuantity < p.Quantity)
            .ToListAsync(ct);

        foreach (var position in positions)
        {
            position.AvailableQuantity = position.Quantity;
            position.UpdatedAt = DateTime.Now;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    #region 私有方法

    private async Task<StockTradeRecord> ExecuteBuyAsync(
        TradingAccount account,
        ExecuteTradeRequest request,
        decimal tradeAmount,
        CancellationToken ct)
    {
        // 买入总支出 = 成交金额 + 手续费 + 印花税
        var totalCost = tradeAmount + request.Fee + request.Tax;

        // 检查可用余额
        if (account.AvailableBalance < totalCost)
        {
            throw new InvalidOperationException(
                $"可用余额不足，需要 {totalCost:F2}，当前 {account.AvailableBalance:F2}");
        }

        // 扣减可用余额
        account.AvailableBalance -= totalCost;

        // 获取或创建持仓
        var position = await _dbContext.StockPositions
            .FirstOrDefaultAsync(p => p.AccountId == request.AccountId && p.Symbol == request.Symbol, ct);

        if (position == null)
        {
            // 新建持仓（T+1：当天买入不可用）
            position = new StockPosition
            {
                AccountId = request.AccountId,
                Symbol = request.Symbol,
                Quantity = request.Quantity,
                AvailableQuantity = 0,          // T+1，当天不可用
                AverageCost = request.Price,    // 首次买入，成本就是买入价
                TotalCost = tradeAmount,
                ProfitLoss = 0,
                ProfitLossRate = 0,
            };
            _dbContext.StockPositions.Add(position);
        }
        else
        {
            // 更新平均成本（加权平均）
            // 新平均成本 = (原总成本 + 本次成交金额) / (原数量 + 本次数量)
            var newTotalCost = position.TotalCost + tradeAmount;
            var newQuantity = position.Quantity + request.Quantity;

            position.AverageCost = newTotalCost / newQuantity;
            position.Quantity = newQuantity;
            position.TotalCost = newTotalCost;
            // AvailableQuantity 不变（T+1，今天买的还不可用）
        }

        // 写入交易记录
        var record = new StockTradeRecord
        {
            AccountId = request.AccountId,
            Symbol = request.Symbol,
            Direction = TradeDirection.Buy,
            Price = request.Price,
            Quantity = request.Quantity,
            Amount = tradeAmount,
            Fee = request.Fee,
            Tax = request.Tax,
            TotalAmount = -totalCost,   // 买入为负，表示资金流出
            TradeTime = request.TradeTime,
            Remark = request.Remark,
        };
        _dbContext.StockTradeRecords.Add(record);

        return record;
    }

    private async Task<StockTradeRecord> ExecuteSellAsync(
        TradingAccount account,
        ExecuteTradeRequest request,
        decimal tradeAmount,
        CancellationToken ct)
    {
        // 卖出净收入 = 成交金额 - 手续费 - 印花税
        var netIncome = tradeAmount - request.Fee - request.Tax;

        // 获取持仓
        var position = await _dbContext.StockPositions
            .FirstOrDefaultAsync(p => p.AccountId == request.AccountId && p.Symbol == request.Symbol, ct)
            ?? throw new InvalidOperationException($"持仓不存在: {request.Symbol}");

        // 检查可用数量（T+1：只能用可用的部分卖出）
        if (position.AvailableQuantity < request.Quantity)
        {
            throw new InvalidOperationException(
                $"可用数量不足，需要 {request.Quantity}，当前可用 {position.AvailableQuantity}，总持仓 {position.Quantity}");
        }

        // 增加可用余额
        account.AvailableBalance += netIncome;

        // 计算本次卖出已实现盈亏
        // 已实现盈亏 = (卖出价 - 平均成本) × 卖出数量
        var realizedProfit = (request.Price - position.AverageCost) * request.Quantity;

        // 更新持仓
        position.Quantity -= request.Quantity;
        position.AvailableQuantity -= request.Quantity;
        position.TotalCost = position.AverageCost * position.Quantity;
        position.ProfitLoss += realizedProfit;
        position.ProfitLossRate = position.TotalCost > 0
            ? position.ProfitLoss / position.TotalCost
            : 0;

        // 更新账户总盈亏
        account.TotalProfitLoss += realizedProfit;

        // 全部清仓则删除持仓记录
        if (position.Quantity <= 0)
        {
            _dbContext.StockPositions.Remove(position);
        }

        // 写入交易记录
        var record = new StockTradeRecord
        {
            AccountId = request.AccountId,
            Symbol = request.Symbol,
            Direction = TradeDirection.Sell,
            Price = request.Price,
            Quantity = request.Quantity,
            Amount = tradeAmount,
            Fee = request.Fee,
            Tax = request.Tax,
            TotalAmount = netIncome,   // 卖出为正，表示资金流入
            TradeTime = request.TradeTime,
            Remark = request.Remark,
        };
        _dbContext.StockTradeRecords.Add(record);

        return record;
    }

    #endregion
}
