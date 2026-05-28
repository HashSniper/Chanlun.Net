using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Extensions
{
    public static class ChanLineExtensions
    {
        private static int GetSpan(this ChanKLine start, ChanKLine end)
        {
            var span = end.Idx - start.Idx;
            //var tempStart = start.Next;
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
            return start.CheckSpanValid(end) &&
                   start.CheckFxValid(end) &&
                   start.CheckEndIsPeak(end);
        }

        private static bool CheckEndIsPeak(this ChanKLine start, ChanKLine end)
        {
            var current = start;
            while (current != null && current.Idx < end.Idx)
            {
                if (current.FX != end.FX)
                {
                    current = current.Next;
                    continue;
                }

                // 如果下降笔，则只要保证 底是最低的，
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
            return start.FX != end.FX && start.FX != ChanFX.UNKNOWN && end.FX != ChanFX.UNKNOWN;
        }
    }
}