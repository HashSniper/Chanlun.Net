using Stock.Data.Entities;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// 海龟交易法则指标处理器 —— 唐奇安通道突破系统
/// 
/// 计算最后一根K线相对于前20日和50日最高/最低价的突破情况。
/// 经典海龟系统使用20日突破作为入场信号，50日突破作为过滤条件。
/// </summary>
public class TurtleTradingIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "TurtleTrading";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        if (klines.Count == 0)
            return Task.CompletedTask;

        int lastIndex = klines.Count - 1;
        var lastItem = items[lastIndex];
        var lastKline = klines[lastIndex];

        // 计算20日唐奇安通道（需要至少21根K线：20根历史 + 1根当前）
        if (klines.Count >= 21)
        {
            var window20 = klines.Take(lastIndex).TakeLast(20).ToList();
            lastItem.TurtleTrading.High20 = window20.Max(k => k.High);
            lastItem.TurtleTrading.Low20 = window20.Min(k => k.Low);
            lastItem.TurtleTrading.BreakoutHigh20 = lastKline.High > lastItem.TurtleTrading.High20;
            lastItem.TurtleTrading.BreakdownLow20 = lastKline.Low < lastItem.TurtleTrading.Low20;
        }

        // 计算50日唐奇安通道（需要至少51根K线）
        if (klines.Count >= 51)
        {
            var window50 = klines.Take(lastIndex).TakeLast(50).ToList();
            lastItem.TurtleTrading.High50 = window50.Max(k => k.High);
            lastItem.TurtleTrading.Low50 = window50.Min(k => k.Low);
            lastItem.TurtleTrading.BreakoutHigh50 = lastKline.High > lastItem.TurtleTrading.High50;
            lastItem.TurtleTrading.BreakdownLow50 = lastKline.Low < lastItem.TurtleTrading.Low50;
        }

        // 生成信号描述
        (lastItem.TurtleTrading.Signal, lastItem.TurtleTrading.IsBullish) =
            GenerateSignal(lastItem.TurtleTrading);

        return Task.CompletedTask;
    }

    private static (string signal, bool? isBullish) GenerateSignal(TurtleTradingValue t)
    {
        // 同时突破20日和50日高点 —— 强多头信号（经典海龟入场）
        if (t.BreakoutHigh20 && t.BreakoutHigh50)
            return ("🐢 海龟突破：价格同时突破20日与50日最高价，强势多头信号，可考虑入场做多", true);

        // 仅突破20日高点 —— 多头信号（需50日过滤时可能不入场）
        if (t.BreakoutHigh20)
        {
            if (t.High50.HasValue)
                return ("📈 突破20日高点：价格突破20日最高价，但未突破50日最高价，中等强度多头信号", true);
            return ("📈 突破20日高点：价格突破20日最高价，数据不足以判断50日通道", true);
        }

        // 同时跌破20日和50日低点 —— 强空头信号
        if (t.BreakdownLow20 && t.BreakdownLow50)
            return ("🐻 海龟跌破：价格同时跌破20日与50日最低价，强势空头信号，可考虑离场或做空", false);

        // 仅跌破20日低点 —— 空头信号
        if (t.BreakdownLow20)
        {
            if (t.Low50.HasValue)
                return ("📉 跌破20日低点：价格跌破20日最低价，但未跌破50日最低价，中等强度空头信号", false);
            return ("📉 跌破20日低点：价格跌破20日最低价，数据不足以判断50日通道", false);
        }

        // 在通道内运行
        bool has20 = t.High20.HasValue && t.Low20.HasValue;
        bool has50 = t.High50.HasValue && t.Low50.HasValue;

        if (!has20 && !has50)
            return ("数据不足（需至少21根K线），无法计算唐奇安通道", null);

        if (has50)
            return ($"通道内运行：20日高低点[{t.High20:F2},{t.Low20:F2}]，50日高低点[{t.High50:F2},{t.Low50:F2}]，暂无突破信号", null);

        return ($"通道内运行：20日高低点[{t.High20:F2},{t.Low20:F2}]，暂无突破信号", null);
    }
}
