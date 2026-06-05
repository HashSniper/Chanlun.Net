using System.Text.Json;
using TdxQuantNet.Interfaces;
using TdxQuantNet.Models;

namespace TdxQuantNet;

public partial class TdxQuant : ITdxQuantTrading
{
    public int StockAccount(string account, string accountType = "stock")
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "0",
            account,
            account_type = accountType
        }, Helper.InputJsonSerializerOptions);

        var ptr = GetOrderStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ StockAccount failed {error}");
        var result = response["Value"]?.GetValue<int>();
        return result ?? throw new Exception($"TQ StockAccount failed {error}");
    }

    public StockAssetModel QueryStockAsset(int accountId)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "0",
            account_id = accountId
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetOrderStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson<StockAssetModel>(ptr, out var error);
        return response ?? throw new Exception($"TQ QueryStockAsset failed {error}");
    }

    public StockOrderItem[] QueryStockOrders(int accountId, string code, bool cancelableOnly)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = 2,
            account_id = accountId,
            stock_code = code,
            cancelable_only = cancelableOnly
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetOrderStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ QueryStockOrders failed {error}");
        var result = response["Value"]
            .Deserialize<StockOrderItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ QueryStockOrders failed {error}");
    }

    public StockPositionsItem[] QueryStockPositions(int accountId)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = 3,
            account_id = accountId
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetOrderStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ QueryStockPositions failed {error}");
        var result = response["Value"]
            .Deserialize<StockPositionsItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ QueryStockPositions failed {error}");
    }

    public OrderStockModel OrderStock(int accountId, string code, int orderType, int orderVolume, int priceType,
        decimal price)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = 4,
            account_id = accountId,
            stock_code = code,
            order_type = orderType,
            order_volume = orderVolume,
            price_type = priceType,
            price
        }, Helper.InputJsonSerializerOptions);

        var ptr = GetOrderStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson<OrderStockModel>(ptr, out var error);
        return response ?? throw new Exception($"TQ OrderStock failed {error}");
    }

    public bool CancelOrderStock(int accountId, string code, string orderId)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = 5,
            account_id = accountId,
            stock_code = code,
            order_id = orderId
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetOrderStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ CancelOrderStock failed {error}");
        return response["Value"]?.GetValue<bool>() ?? false;
    }
}
