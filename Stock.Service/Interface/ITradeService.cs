using Stock.Data.Entities;
using Stock.Service.Entitys;

namespace Stock.Service.Interface;

/// <summary>
/// 交易服务接口 —— 执行买入/卖出，T+1结算
/// </summary>
public interface ITradeService
{
    /// <summary>
    /// 执行一笔交易（买入或卖出）
    /// </summary>
    Task<StockTradeRecord> ExecuteTradeAsync(ExecuteTradeRequest request, CancellationToken ct = default);

    /// <summary>
    /// T+1 结算：将昨日买入的不可用持仓变为可用
    /// </summary>
    Task SettlementAsync(long accountId, CancellationToken ct = default);
}
