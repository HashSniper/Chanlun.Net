using System.Text.Json.Nodes;
using TdxQuantNet.Models;

namespace TdxQuantNet.Interfaces;

public interface ITdxQuantMarket
{
    /// <summary>
    /// 获取K线行情
    /// </summary>
    /// <param name="code">证券代码</param>
    /// <param name="startDate">起始时间</param>
    /// <param name="endDate">结束时间</param>
    /// <param name="period">周期</param>
    /// <param name="dividendType">复权类型</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取K线行情get_market_data.md"/>
    /// </remarks>
    public MarketDataModel GetMarketData(
        string code,
        string startDate,
        string? endDate = null,
        string? period = null,
        string? dividendType = null
    );

    /// <summary>
    /// 获取K线行情
    /// </summary>
    /// <param name="code">证券代码</param>
    /// <param name="count">返回数据个数</param>
    /// <param name="period">周期</param>
    /// <param name="endDate">结束时间</param>
    /// <param name="dividendType">复权类型</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取K线行情get_market_data.md"/>
    /// </remarks>
    public MarketDataModel GetMarketData(
        string code,
        int count,
        string? period = null,
        string? endDate = null,
        string? dividendType = null
    );
    
    /// <summary>
    /// 获取快照数据
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取快照数据get_market_snapshot.md"/>
    /// </remarks>
    public MarketSnapshotModel GetMarketSnapshot(string code);

    /// <summary>
    /// 获取证券基本信息
    /// </summary>
    /// <param name="code">证券代码</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取证券基本信息get_stock_info.md"/>
    /// </remarks>
    public JsonNode GetStockInfo(string code);
    
    /// <summary>
    /// 获取股票更多信息
    /// </summary>
    /// <param name="code">股票代码</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取股票更多信息get_more_info.md"/>
    /// </remarks>
    public JsonNode GetMoreInfo(string code);

    /// <summary>
    /// 获取分红配送数据
    /// </summary>
    /// <param name="code">证券代码</param>
    /// <param name="startDate">起始时间</param>
    /// <param name="endDate">结束时间</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取分红配送数据get_divid_factors.md"/>
    /// </remarks>
    public DividFactorsItem[] GetDividFactors(string code, string startDate, string? endDate = null);

    /// <summary>
    /// 获取股票所属板块
    /// </summary>
    /// <param name="code">股票代码</param>
    /// <returns></returns>
    public RelationItem[] GetRelation(string code);

    /// <summary>
    /// 获取新股申购信息
    /// </summary>
    /// <param name="ipoType">自定义板块简称</param>
    /// <param name="ipoDate">自定义板块名称</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取新股申购信息get_ipo_info.md"/>
    /// </remarks>
    public IpoInfoItem[] GetIpoInfo(int ipoType, int ipoDate);

    /// <summary>
    /// 获取每天的股本数据
    /// </summary>
    /// <param name="code">股票代码</param>
    /// <param name="dateList"></param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/行情类信息/获取每天的股本数据get_gb_info.md"/>
    /// </remarks>
    public GbInfoItem[] GetGbInfo(string code, params string[] dateList);
}
