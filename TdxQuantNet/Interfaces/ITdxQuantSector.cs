using TdxQuantNet.Models;

namespace TdxQuantNet.Interfaces;

/// <summary>
/// 分类/板块成份股
/// </summary>
/// <see href="https://help.tdx.com.cn/quant/docs/markdown/mindoc-1ctuhttn72svo/">help.tdx.com.cn</see>
public interface ITdxQuantSector
{
    /// <summary>
    /// 获取系统分类成份股
    /// </summary>
    /// <param name="market">指定代码</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/分类-板块成份股/获取系统分类成份股get_stock_list.md"/>
    /// </remarks>
    public SectorItem[] GetStockList(string market);

    /// <summary>
    /// 获取A股板块代码列表
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/分类-板块成份股/获取A股板块代码列表get_stock_list_in_sector.md"/>
    /// </remarks>
    public SectorItem[] GetSectorList();

    /// <summary>
    /// 获取板块成份股
    /// </summary>
    /// <param name="blockCode">板块代码</param>
    /// <param name="blockType">板块类型</param>
    /// <returns></returns>
    /// <remarks>
    /// <see href="https://github.com/afute/TdxQuantNet/blob/main/Docs/分类-板块成份股/获取板块成份股get_stock_list_in_sector.md"/>
    /// </remarks>
    public SectorItem[] GetStockListInSector(string blockCode, int? blockType = null);
}