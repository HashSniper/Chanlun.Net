using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.Extensions;

public static class ChanExtensions
{
    public static bool IsUp(this ChanDir dir)
    {
        return dir == ChanDir.UP;
    }
    
    public static bool IsDown(this ChanDir dir)
    {
        return dir == ChanDir.DOWN;
    }
    
    public static bool IsUp(this ChanDir? dir)
    {
        return dir == ChanDir.UP;
    }
    
    public static bool IsDown(this ChanDir? dir)
    {
        return dir == ChanDir.DOWN;
    }

    public static ChanDir RevertDir(this ChanDir dir)
    {
        return dir.IsUp() ? ChanDir.DOWN : ChanDir.UP;
    }
}