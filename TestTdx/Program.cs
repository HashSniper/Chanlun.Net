using TdxQuantNet;

using var tq = TdxQuantFactory.Create("tq.net.test1", @"C:\new_tdx64");
var res = tq.GetMarketData("688318.SH",-1);
Console.WriteLine(res);