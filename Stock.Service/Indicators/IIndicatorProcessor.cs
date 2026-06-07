using Stock.Data.Entities;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// 指标处理器接口 —— 每个指标实现此接口，负责独立计算并填充结果
/// </summary>
public interface IIndicatorProcessor
{
    /// <summary>指标名称</summary>
    string Name { get; }

    /// <summary>
    /// 异步计算指标并填充到每个 KLineIndicatorItem 中
    /// </summary>
    /// <param name="klines">原始K线数据</param>
    /// <param name="items">结果列表（已按索引与klines对齐）</param>
    /// <param name="ct">取消令牌</param>
    Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default);
}
