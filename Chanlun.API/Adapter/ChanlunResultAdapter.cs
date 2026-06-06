using Chanlun.API.Models;
using Chanlun.Lib;
using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;

namespace Chanlun.API.Adapter;

/// <summary>
/// 将 ChanCalculateResult 转换为前端可用的 TvChanlunResponse
/// </summary>
public static class ChanlunResultAdapter
{
    public static TvChanlunResponse ConvertToTvResponse(int barCount, ChanCalculateResult result)
    {
        var response = new TvChanlunResponse
        {
            BarCount = barCount,
            Symbol = result.Symbol ?? string.Empty
        };

        // K线数据
        if (result.UnitList?.Count > 0)
        {
            response.Bars = result.UnitList.Select(u => new TvKlineBar
            {
                Time = u.Time,
                Open = u.Open,
                High = u.High,
                Low = u.Low,
                Close = u.Close,
                Volume = u.Volume
            }).ToList();
        }

        // 笔
        if (result.BiList.IsNotNullOrEmpty())
        {
            foreach (var bi in result.BiList)
            {
                var startUnit = bi.StartChanKLine.PeakUnit;
                var endUnit = bi.EndChanKLine.PeakUnit;
                var startPrice = bi.DIR.IsUp() ? bi.StartChanKLine.Low : bi.StartChanKLine.High;
                var endPrice = bi.DIR.IsUp() ? bi.EndChanKLine.High : bi.EndChanKLine.Low;

                response.BiList.Add(new TvBiItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    StartPrice = startPrice,
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    EndPrice = endPrice,
                    Direction = bi.DIR.IsUp() ? "up" : "down"
                });
            }
        }

        // 线段
        if (result.SegList.IsNotNullOrEmpty())
        {
            foreach (var seg in result.SegList)
            {
                var startUnit = seg.StartBi.StartChanKLine.PeakUnit;
                var endUnit = seg.EndBi.EndChanKLine.PeakUnit;
                var startPrice = seg.DIR.IsUp() ? seg.Low : seg.High;
                var endPrice = seg.DIR.IsUp() ? seg.High : seg.Low;

                response.SegList.Add(new TvSegItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    StartPrice = startPrice,
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    EndPrice = endPrice,
                    Direction = seg.DIR.IsUp() ? "up" : "down"
                });
            }
        }

        // 笔中枢
        if (result.BiPivotList.IsNotNullOrEmpty())
        {
            foreach (var pivot in result.BiPivotList)
            {
                var startUnit = pivot.Segments[0].StartChanKLine.PeakUnit;
                var endUnit = pivot.Segments[^1].EndChanKLine.PeakUnit;

                response.BiPivotList.Add(new TvPivotItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    ZG = pivot.ZG,
                    ZD = pivot.ZD,
                    GG = pivot.GG,
                    DD = pivot.DD,
                    Type = "bi",
                    Level = pivot.Level
                });
            }
        }

        // 线段中枢
        if (result.SegPivotList.IsNotNullOrEmpty())
        {
            foreach (var pivot in result.SegPivotList)
            {
                var startUnit = pivot.Segments[0].StartBi.StartChanKLine.PeakUnit;
                var endUnit = pivot.Segments[^1].EndBi.EndChanKLine.PeakUnit;

                response.SegPivotList.Add(new TvPivotItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    ZG = pivot.ZG,
                    ZD = pivot.ZD,
                    GG = pivot.GG,
                    DD = pivot.DD,
                    Type = "seg",
                    Level = pivot.Level
                });
            }
        }

        // 合并K线
        if (result.LineList.IsNotNullOrEmpty())
        {
            foreach (var kline in result.LineList)
            {
                if (kline.CombinedUnits.Count == 0) continue;

                var startUnit = kline.CombinedUnits[0];
                var endUnit = kline.CombinedUnits[^1];

                response.MergedKLines.Add(new TvMergedKLine
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    High = kline.High,
                    Low = kline.Low,
                    Direction = kline.DIR == ChanDir.UP ? "up" : kline.DIR == ChanDir.DOWN ? "down" : "combine"
                });
            }
        }

        return response;
    }
}
