using TdxQuantNet;
using Chanlun.Lib.ChanCommon;

var td = Utils.ConvertToDateTime(1260605, 1500);

using var tq = TdxQuantFactory.Create("tq.net.test1", @"C:\new_tdx64");
var res = tq.GetMarketData("688318.SH",-1);

Console.WriteLine(res);