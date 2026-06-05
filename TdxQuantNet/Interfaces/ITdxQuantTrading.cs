using TdxQuantNet.Models;

namespace TdxQuantNet.Interfaces;

/// <summary>
/// 交易函数
/// </summary>
/// <see href="https://help.tdx.com.cn/quant/docs/markdown/mindoc-1h7k4iqb1grk4/">help.tdx.com.cn</see>
public interface ITdxQuantTrading
{
    /// <summary>
    /// 获取资金账户句柄
    /// </summary>
    /// <param name="account">资金账号</param>
    /// <param name="accountType">账号类型</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/交易函数/获取资金账户句柄stock_account.md"/>
    /// </remarks>
    public int StockAccount(string account, string accountType = "stock");

    /// <summary>
    /// 查询账户资产信息
    /// </summary>
    /// <param name="accountId">资金账号句柄</param>
    /// <returns></returns>
    public StockAssetModel QueryStockAsset(int accountId);

    /// <summary>
    /// 查询账户委托信息
    /// </summary>
    /// <param name="accountId">资金账号句柄</param>
    /// <param name="code">证券代码</param>
    /// <param name="cancelableOnly">是否仅查询可撤委托</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/交易函数/查询账户委托信息query_stock_orders.md"/>
    /// </remarks>
    public StockOrderItem[] QueryStockOrders(int accountId, string code, bool cancelableOnly);

    /// <summary>
    /// 查询账户持仓信息
    /// </summary>
    /// <param name="accountId">资金账号句柄</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/交易函数/查询账户持仓信息query_stock_positions.md"/>
    /// </remarks>
    public StockPositionsItem[] QueryStockPositions(int accountId);

    /// <summary>
    /// 交易执行函数
    /// </summary>
    /// <param name="accountId">资金账号句柄</param>
    /// <param name="code">证券代码</param>
    /// <param name="orderType">委托类型</param>
    /// <param name="orderVolume">委托数量</param>
    /// <param name="priceType">报价类型</param>
    /// <param name="price">委托价格</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/交易函数/交易执行函数order_stock.md"/>
    /// </remarks>
    public OrderStockModel OrderStock(int accountId, string code, int orderType, int orderVolume, int priceType,
        decimal price);

    /// <summary>
    /// 撤单
    /// </summary>
    /// <param name="accountId">资金账号句柄</param>
    /// <param name="code">证券代码</param>
    /// <param name="orderId">委托编号</param>
    /// <returns></returns>
    public bool CancelOrderStock(int accountId, string code, string orderId);
}
