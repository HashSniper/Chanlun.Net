using Chan.Lib.Common;

namespace Chan.Lib.KLines;

public class TradeInfo
{
    public Dictionary<string, double?> Metric { get; } = new();

    public TradeInfo(Dictionary<string, object> info)
    {
        foreach (var metricName in DATA_FIELD.TRADE_INFO_LST)
        {
            if (info.TryGetValue(metricName, out var value) && value is double d)
                Metric[metricName] = d;
            else if (value is float f)
                Metric[metricName] = f;
            else if (value is int i)
                Metric[metricName] = i;
            else
                Metric[metricName] = null;
        }
    }

    public override string ToString()
    {
        return string.Join(" ", Metric.Select(kv => $"{kv.Key}:{kv.Value}"));
    }
}
