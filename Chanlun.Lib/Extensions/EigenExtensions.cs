using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib.Extensions;

public static class EigenExtensions
{
    /// <summary>
    /// true：表示不能合并
    /// </summary>
    /// <param name="self"></param>
    /// <param name="bi"></param>
    /// <returns></returns>
    public static ChanDir? GetNewDirWithBi(this Eigen self, Bi.Bi bi)
    {
        if (self == null) throw new ArgumentNullException(nameof(self));
        var dir = self.GetNewNodeDir(bi);

        if (dir == null)
        {
            self.AddBi(bi);
        }

        return dir;
    }

    public static void UpdateFx(this Eigen self, Eigen pre, Eigen next)
    {
        self.Next = next;
        self.Pre = pre;
        next.Pre = self;
        pre.Next = self;

        if ((self.FX == ChanFX.TOP && pre.High < self.Low) || (self.FX == ChanFX.BOTTOM && pre.Low > self.High))
        {
            self.Gap = true;
        }

    }
}