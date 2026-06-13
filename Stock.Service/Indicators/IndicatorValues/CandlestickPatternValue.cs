using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>蜡烛图形态指标值</summary>
public class CandlestickPatternValue
{
    public CandlestickPattern Patterns { get; set; }
    public List<CandlestickPatternInfo> PatternDetails { get; set; } = [];
    public PatternDirection PatternDirection { get; set; }
    public string PatternSignal { get; set; } = string.Empty;
}
