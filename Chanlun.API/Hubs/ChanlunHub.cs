using Microsoft.AspNetCore.SignalR;

namespace Chanlun.API.Hubs;

public class ChanlunHub : Hub
{
    /// <summary>
    /// 通知所有连接的客户端：通达信当前窗口数据已更新，可刷新同步
    /// </summary>
    public const string TdxDataUpdatedMethod = "TdxDataUpdated";
}
