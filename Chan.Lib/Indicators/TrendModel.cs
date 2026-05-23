using Chan.Lib;
using Chan.Lib.Common;

namespace Chan.Lib.Indicators;

public class TrendModel
{
    public int T { get; }
    public TREND_TYPE Type { get; }
    private readonly List<double> _arr = new();

    public TrendModel(TREND_TYPE trendType, int t)
    {
        T = t;
        Type = trendType;
    }

    public double Add(double value)
    {
        _arr.Add(value);
        if (_arr.Count > T)
            _arr.RemoveAt(0);

        return Type switch
        {
            TREND_TYPE.MEAN => _arr.Average(),
            TREND_TYPE.MAX => _arr.Max(),
            TREND_TYPE.MIN => _arr.Min(),
            _ => throw new ChanException($"Unknown trendModel Type = {Type}", ErrCode.PARA_ERROR)
        };
    }
}
