using TdxQuantNet.Models;

namespace TdxQuantNet.Interfaces;

public interface ITdxQuantDerived
{
    /// <summary>
    /// 获取跟踪指数的ETF信息
    /// </summary>
    /// <param name="zsCode">指数代码</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/ETF-可转债-期货数据/获取跟踪指数的ETF信息get_trackzs_etf_info.md"/>
    /// </remarks>
    public EtfInfoItem[] GetTrackzsEtfInfo(string zsCode);

    /// <summary>
    /// 获取可转债信息
    /// </summary>
    /// <param name="stockCode">可转债代码</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/ETF-可转债-期货数据/获取可转债信息get_kzz_info.md"/>
    /// </remarks>
    public KzzInfoItem GetKzzInfo(string stockCode);
}
