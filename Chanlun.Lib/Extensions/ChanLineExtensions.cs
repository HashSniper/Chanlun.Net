using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Extensions
{
    public static class ChanLineExtensions
    {
        
        public static int GetSpan(this ChanKLine start, ChanKLine end)
        {
            var span = end.Idx - start.Idx;
            var tempStart = start.Next;
            // while (tempStart != null && tempStart.Idx < end.Idx)
            // {
            //     //跳空缺口，可以多一根
            //     if (tempStart.HasGap(tempStart.Next))
            //     {
            //         span++;
            //     }
            //
            //     tempStart = tempStart.Next;
            // }

            return span;
        }

        public static bool CanCreateNewBi(this ChanKLine start, ChanKLine end)
        {
            if (!start.CheckSpanValid(end))
            {
                return false;
            }

            return start.CheckFxValid(end) && start.CheckEndIsPeak(end);
        }

        private static bool CheckEndIsPeak(this ChanKLine start, ChanKLine end)
        {
            var current = start;
            while (current != null && current.Idx < end.Idx)
            {
                if (start.FX == ChanFX.TOP && current.Low < end.Low)
                {
                    return false;
                }
                else if (start.FX == ChanFX.BOTTOM && current.High > end.High)
                {
                    return false;
                }

                current = current.Next;
            }

            return true;
        }

        private static bool CheckSpanValid(this ChanKLine start, ChanKLine end)
        {
            var span = start.GetSpan(end);
            //一笔至少5 k线
            return span >= 4;
        }

        private static bool CheckFxValid(this ChanKLine start, ChanKLine end)
        {
            if (start.FX == end.FX || start.FX == ChanFX.UNKNOWN || end.FX == ChanFX.UNKNOWN )
            {
                return false;
            }

            switch (start.FX)
            {
                case ChanFX.TOP:
                {
                    var endMaxHigh = Math.Max(end.Pre.High, end.High);
                    var startMinLow = Math.Min(start.Low, start.Next.Low);
                    return start.High > endMaxHigh && end.Low < startMinLow;
                }
                case ChanFX.BOTTOM:
                {
                    var endMinLow = Math.Min(end.Pre.Low, end.Low);
                    var startMaxHigh = Math.Max(start.High, start.Next.High);
                    return start.Low < endMinLow && end.High > startMaxHigh;
                }
                default:
                    return false;
            }
        }
    }
}