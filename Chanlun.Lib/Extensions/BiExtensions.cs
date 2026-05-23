using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Extensions;

public static class BiExtensions
{
    public static void RestoreToSureEnd(this Bi.Bi self, KLineGroup end)
    {
        self.IsSure = true;
        self.UpdateNewEnd(end);
        self.SureEndKLines.Clear();
    }

    private static void UpdateNewEnd(this Bi.Bi self, KLineGroup end)
    {
        self.EndKLine = end;
    }


    public static bool TryUpdateEnd(this Bi.Bi self, KLineGroup end)
    {
        if ((self.DIR == ChanDir.UP && end.DIR == ChanDir.UP && end.High >= self.EndKLine.High) ||
            (self.DIR == ChanDir.DOWN && end.DIR == ChanDir.DOWN && end.Low <= self.EndKLine.Low))
        {
            self.UpdateNewEnd(end);
            return true;
        }

        return false;
    }

    public static void UpdateVirtualEnd(this Bi.Bi self, KLineGroup end)
    {
        self.SureEndKLines.Add(self.EndKLine);
        self.UpdateNewEnd(end);
        self.IsSure = false;
    }

    public static float GetEndValue(this Bi.Bi self)
    {
        return self.DIR == ChanDir.UP ? self.EndKLine.High : self.EndKLine.Low;
    }
}