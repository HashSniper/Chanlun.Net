using System.Text.Json.Nodes;
using TdxQuantNet.Models;

namespace TdxQuantNet.Interfaces;

public interface ITdxQuantFormula
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public FormatKLine[] FormulaFormatData(MarketDataModel data);

    /// <summary>
    /// 向通达信公式设置数据
    /// </summary>
    /// <param name="stockCode"></param>
    /// <param name="stockPeriod"></param>
    /// <param name="stockData"></param>
    /// <param name="dividendType"></param>
    /// <returns></returns>
    public bool FormulaSetData(string stockCode, string stockPeriod, FormatKLine[] stockData, int dividendType);

    /// <summary>
    /// 向通达信公式设置数据
    /// </summary>
    /// <param name="stockCode"></param>
    /// <param name="stockPeriod"></param>
    /// <param name="stockData"></param>
    /// <param name="dividendType"></param>
    /// <returns></returns>
    public bool FormulaSetData(string stockCode, string stockPeriod, MarketDataModel stockData, int dividendType);

    /// <summary>
    /// 向通达信公式设置数据信息
    /// </summary>
    /// <param name="stockCode"></param>
    /// <param name="stockPeriod"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="count"></param>
    /// <param name="dividendType"></param>
    /// <returns></returns>
    public bool FormulaSetDataInfo(string stockCode, string stockPeriod, string startDate, string endDate, int count,
        int dividendType);

    /// <summary>
    /// 获取公式中的设置数据
    /// </summary>
    /// <returns></returns>
    public JsonNode FormulaGetData();

    /// <summary>
    /// 调用通达信公式进行计算
    /// </summary>
    /// <param name="formulaType"></param>
    /// <param name="formulaName"></param>
    /// <param name="xsFlag"></param>
    /// <param name="formulaArg"></param>
    /// <returns></returns>
    public JsonNode TdxFormula(int formulaType, string formulaName, int xsFlag, params int[] formulaArg);

    public JsonNode FormulaProcessMul(string formulaName,
        string[] formulaArg,
        int formulaType,
        int returnCount,
        bool returnDate,
        int xsFlag,
        string[] stockList,
        string stockPeriod,
        string startTime,
        string endTime,
        int count,
        int dividendType
    );
}
