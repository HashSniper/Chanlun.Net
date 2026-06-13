namespace Stock.Data.Entities;

/// <summary>
/// 数据同步状态
/// </summary>
public enum SyncStatus
{
    /// <summary>未同步</summary>
    NotSynced = 0,

    /// <summary>同步中</summary>
    Syncing = 1,

    /// <summary>已同步</summary>
    Synced = 2
}
