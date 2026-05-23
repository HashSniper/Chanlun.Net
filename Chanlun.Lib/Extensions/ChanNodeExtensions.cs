using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Extensions;

public static class ChanNodeExtensions
{
    public static ChanDir? GetNewNodeDir<T1,T2>(this ChanNode<T1> current, ChanNode<T2> newNode) where T1 : class  where T2 : class
    {
        if (current.High > newNode.High && current.Low > newNode.Low)
        {
            return ChanDir.DOWN;
        }

        if (current.High < newNode.High && current.Low < newNode.Low)
        {
            return ChanDir.UP;
        }

        return null;
    }
    
    public static bool HasGap<T>(this ChanNode<T> self, ChanNode<T>? otherNode) where T : class 
    {
        if (otherNode== null) return false;
        return !(otherNode.High >= self.Low && self.High >= otherNode.Low);
    }
}