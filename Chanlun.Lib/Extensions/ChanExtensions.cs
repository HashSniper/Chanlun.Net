using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.Extensions;

public static class ChanExtensions
{
    public static bool IsUp(this ChanDir dir)
    {
        return dir == ChanDir.UP;
    }
}