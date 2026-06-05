using System.Text.Json.Nodes;
using TdxQuantNet.Interfaces;
using TdxQuantNet.Models;

namespace TdxQuantNet;

public partial class TdxQuant : ITdxQuantFormula
{
    public FormatKLine[] FormulaFormatData(MarketDataModel data)
    {
        throw new NotImplementedException();
    }

    public bool FormulaSetData(string stockCode, string stockPeriod, FormatKLine[] stockData, int dividendType)
    {
        throw new NotImplementedException();
    }

    public bool FormulaSetData(string stockCode, string stockPeriod, MarketDataModel stockData, int dividendType)
    {
        throw new NotImplementedException();
    }

    public bool FormulaSetDataInfo(string stockCode, string stockPeriod, string startDate, string endDate, int count,
        int dividendType)
    {
        throw new NotImplementedException();
    }

    public JsonNode FormulaGetData()
    {
        throw new NotImplementedException();
    }

    public JsonNode TdxFormula(int formulaType, string formulaName, int xsFlag, params int[] formulaArg)
    {
        throw new NotImplementedException();
    }

    public JsonNode FormulaProcessMul(string formulaName, string[] formulaArg, int formulaType, int returnCount, bool returnDate,
        int xsFlag, string[] stockList, string stockPeriod, string startTime, string endTime, int count, int dividendType)
    {
        throw new NotImplementedException();
    }
}
