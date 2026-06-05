using TdxQuantNet.Models;

namespace TdxQuantNet.Interfaces;

public interface ITdxQuantOptional
{
    /// <summary>
    /// 获取自定义板块列表
    /// </summary>
    /// <returns></returns>
    public SectorItem[] GetUserSector();

    /// <summary>
    /// 添加自定义板块成份股
    /// </summary>
    /// <param name="blockCode">自定义板块简称</param>
    /// <param name="show">客户端是否切换至对应板块界面</param>
    /// <param name="stockList">添加的自选股</param>
    public bool SendUserBlock(string blockCode, bool show = false, params string[] stockList);
    
    /// <summary>
    /// 清空自定义板块成份股
    /// </summary>
    /// <param name="blockCode">自定义板块简称</param>
    public bool ClearSector(string blockCode);
    
    /// <summary>
    /// 创建自定义板块
    /// </summary>
    /// <param name="blockCode">自定义板块简称</param>
    /// <param name="blockName">自定义板块名称</param>
    public bool CreateSector(string blockCode, string blockName);

    /// <summary>
    /// 删除自定义板块
    /// </summary>
    /// <param name="blockCode">自定义板块简称</param>
    public bool DeleteSector(string blockCode);
    
    /// <summary>
    /// 创建自定义板块
    /// </summary>
    /// <param name="blockCode">自定义板块简称</param>
    /// <param name="blockName">重命名后的自定义板块名称</param>
    public bool RenameSctor(string blockCode, string blockName);
}
