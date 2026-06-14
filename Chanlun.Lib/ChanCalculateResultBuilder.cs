using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;
using Chanlun.Lib.SEG;
using Chanlun.Lib.StockIndicators;
using Chanlun.Lib.TradingPoint;
using Chanlun.Lib.Zs;

namespace Chanlun.Lib;

public static class ChanCalculateResultBuilder
{
    public static ChanCalculateResult Build(string symbol, List<KLineUnit> units)
    {
        var result = new ChanCalculateResult()
        {
            Symbol = symbol,
            UnitList = units
        };
        ChanKLineCalculator.Calculate(ref result);
        BiCalculator.Calculate(ref result);
        SegCalculator.Calculate(ref result);
        PivotCalculator.Calculate(ref result); 
        IndicatorCalculator.Calculate(ref result);
        ChanTradingPointCalculator.Calculate(ref result);
        return result;
    }
}