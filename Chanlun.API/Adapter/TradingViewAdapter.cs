using Chanlun.API.Models;
using Chanlun.Lib.ChanCommon;
using TdxQuantNet;

namespace Chanlun.API.Adapter;

public static class TradingViewAdapter
{
    // private static TdxQuant _tdxQuant;
    //
    // static TradingViewAdapter()
    // {
    //     _tdxQuant = TdxQuantFactory.Create("tq.net.test3", @"C:\new_tdx64");
    // }
    //
    // public static List<TvKlineBar> GetKlineBars(string symbol, string resolution, DateTime from, DateTime to)
    // {
    //     var marketdata = _tdxQuant.GetMarketData(symbol, from.ToString("yyyyMMdd"));
    //     List<TvKlineBar> res = new List<TvKlineBar>();
    //     for (var i = 0; i < marketdata.CloseFrame.Length; i++)
    //     {
    //         res.Add(new TvKlineBar()
    //         {
    //             Time = Utils.ConvertToDateTime(marketdata.DateFrame[i], marketdata.TimeFrame[i]),
    //             Open = (decimal)marketdata.OpenFrame[i],
    //             Close = (decimal)marketdata.CloseFrame[i],
    //             High = (decimal)marketdata.HighFrame[i],
    //             Low =  (decimal)marketdata.LowFrame[i],
    //             Volume = marketdata.VolumeFrame[i]
    //         });
    //     }
    //     
    //     return res;
    // }
}