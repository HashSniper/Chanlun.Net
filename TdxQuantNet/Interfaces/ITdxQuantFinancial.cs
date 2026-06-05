using System.Text.Json.Nodes;

namespace TdxQuantNet.Interfaces;

/// <summary>
/// 财务类数据
/// </summary>
/// <see href="https://help.tdx.com.cn/quant/docs/markdown/TdxQuant.md/">help.tdx.com.cn</see>
public interface ITdxQuantFinancial
{
    /// <summary>
    /// 获取专业财务数据
    /// </summary>
    /// <param name="code">证券代码</param>
    /// <param name="start">起始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="reportType">按截止日期还是公告日期筛选</param>
    /// <param name="fieldList">字段筛选</param>
    /// <returns></returns>
    public JsonNode GetFinancialData(string code, string start, string end, string reportType = "announce_time",
        params string[] fieldList);

    /// <summary>
    /// 获取指定日期专业财务数据
    /// </summary>
    /// <param name="code"></param>
    /// <param name="year"></param>
    /// <param name="mmdd"></param>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    public JsonNode GetFinancialDataByDate(string code, int year, int mmdd, params string[] fieldList);
    
    /// <summary>
    /// 获取指定日期股票交易数据
    /// </summary>
    /// <param name="code">证券代码</param>
    /// <param name="year">指定年份</param>
    /// <param name="mmdd">指定月日</param>
    /// <param name="fieldList">字段筛选</param>
    /// <returns></returns>
    public JsonNode GetGpJyValueByDate(string code, int year, int mmdd, params string[] fieldList);

    /// <summary>
    /// 获取板块交易数据
    /// </summary>
    /// <param name="code"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    public JsonNode GetBkJyValue(string code, string start, string end, params string[] fieldList);

    /// <summary>
    /// 获取指定日期板块交易数据
    /// </summary>
    /// <param name="code"></param>
    /// <param name="year"></param>
    /// <param name="mmdd"></param>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    public JsonNode GetBkJyValueByDate(string code, int year, int mmdd, params string[] fieldList);

    /// <summary>
    /// 获取市场交易数据
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    public JsonNode GetScJyValue(string start, string end, params string[] fieldList);
    
    /// <summary>
    /// 获取指定日期市场交易数据
    /// </summary>
    /// <param name="year"></param>
    /// <param name="mmdd"></param>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    public JsonNode GetScJyValueByDate(int year, int mmdd, params string[] fieldList);

    /// <summary>
    /// 获取股票的单个财务数据
    /// </summary>
    /// <param name="code"></param>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    public JsonNode GetGpOneData(string code, params string[] fieldList);
}