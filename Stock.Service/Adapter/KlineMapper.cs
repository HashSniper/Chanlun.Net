using Chanlun.Lib.KLine;
using Stock.Data.Entities;
using Stock.Service.Interface;

namespace Stock.Service.Adapter
{
    /// <summary>
    /// KlineBase 与 KLineUnit 之间的双向映射器
    /// </summary>
    public static class KlineMapper
    {
        #region KLineUnit → KlineBase

        /// <summary>
        /// 将 KLineUnit 列表映射为对应周期的 KlineBase 实体列表
        /// </summary>
        /// <param name="units">KLineUnit 列表</param>
        /// <param name="symbol">股票代码</param>
        /// <returns>KlineBase 列表（具体类型由时间差自动判断）</returns>
        public static List<KlineBase> Map(List<KLineUnit> units, string symbol)
        {
            if (units == null || units.Count == 0)
                return [];

            var resolution = DetectResolution(units);
            return units.Select(u => MapUnit(u, symbol, resolution)).ToList();
        }

        /// <summary>
        /// 通过相邻 K 线的时间差中位数判断周期级别
        /// </summary>
        public static KlineResolution DetectResolution(List<KLineUnit> units)
        {
            if (units.Count < 2)
                return KlineResolution.Day;

            var diffs = units.Zip(units.Skip(1), (a, b) => b.Time - a.Time)
                .Select(d => d.TotalMinutes)
                .ToList();

            // 取中位数，避免休市间隙等异常值干扰
            var medianDiff = diffs.OrderBy(d => d).ElementAt(diffs.Count / 2);

            if (medianDiff < 2)
                return KlineResolution.Minute1;
            if (medianDiff < 10)
                return KlineResolution.Minute5;
            if (medianDiff < 22)
                return KlineResolution.Minute15;
            if (medianDiff < 45)
                return KlineResolution.Minute30;
            if (medianDiff < 90)
                return KlineResolution.Minute60;

            var medianDays = medianDiff / 1440.0;

            if (medianDays < 2)
                return KlineResolution.Day;

            return KlineResolution.Day;
        }

        /// <summary>
        /// 将单个 KLineUnit 映射为对应周期的 KlineBase 实体
        /// </summary>
        private static KlineBase MapUnit(KLineUnit unit, string symbol, KlineResolution resolution)
        {
            KlineBase result = resolution switch
            {
                KlineResolution.Minute1 => new Kline1m(),
                KlineResolution.Minute5 => new Kline5m(),
                KlineResolution.Minute15 => new Kline15m(),
                KlineResolution.Minute30 => new Kline30m(),
                KlineResolution.Minute60 => new Kline60m(),
                KlineResolution.Day => new Kline1d(),
                _ => new Kline1d()
            };

            result.Symbol = symbol;
            result.TradeTime = unit.Time;
            result.Open = unit.Open;
            result.High = unit.High;
            result.Low = unit.Low;
            result.Close = unit.Close;
            result.Volume = unit.Volume;
            result.Amount = unit.Amount;

            return result;
        }

        #endregion

        #region KlineBase → KLineUnit

        /// <summary>
        /// 将单个 KlineBase 映射为 KLineUnit
        /// </summary>
        /// <param name="kline">KlineBase 实体（任意具体周期子类）</param>
        /// <param name="idx">节点索引</param>
        /// <returns>KLineUnit</returns>
        public static KLineUnit ToKLineUnit(this KLineIndicatorItem kline, int idx)
        {
            return new KLineUnit(idx)
            {
                Time = kline.Kline.TradeTime,
                Open = kline.Kline.Open,
                High = kline.Kline.High,
                Low = kline.Kline.Low,
                Close = kline.Kline.Close,
                Volume = kline.Kline.Volume,
                Amount = kline.Kline.Amount,
                UnitIndicator = new KLineUnitIndicator()
                {
                    MACD = kline.Macd,
                    Boll = kline.Boll,
                }
            };
        }

        /// <summary>
        /// 将 KlineBase 列表映射为 KLineUnit 列表，按 TradeTime 排序、去重并自动链接 Pre/Next
        /// </summary>
        /// <param name="klines">KlineBase 列表</param>
        /// <returns>已排序、去重并链接好的 KLineUnit 列表</returns>
        public static List<KLineUnit> ToKLineUnits(this IEnumerable<KLineIndicatorItem> klines)
        {
            var ordered = klines
                .GroupBy(k => k.Kline.TradeTime)
                .Select(g => g.First())
                .OrderBy(k => k.Kline.TradeTime)
                .ToList();
            var result = new List<KLineUnit>(ordered.Count);

            for (int i = 0; i < ordered.Count; i++)
            {
                var unit = ordered[i].ToKLineUnit(i);
                result.Add(unit);
            }

            // 链接 Pre/Next 指针，重建双向链表
            for (int i = 0; i < result.Count; i++)
            {
                if (i > 0)
                    result[i].Pre = result[i - 1];
                if (i < result.Count - 1)
                    result[i].Next = result[i + 1];
            }

            return result;
        }

        #endregion
    }
}