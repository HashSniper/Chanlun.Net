using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.KLine
{
    public class ChanKLineList : List<ChanKLine>
    {
        private ChanKLine? LastKLine => this.IsNotNullOrEmpty() ? this.Last() : null;

        public ChanKLine CreateOrUpdateKLineCombineFromUnit(KLineUnit kLineUnit)
        {
            if (LastKLine == null)
            {
                return CreateNewLine(kLineUnit, ChanDir.UP);
            }

            var currentDir = LastKLine.GetNewNodeDir(kLineUnit);

            if (currentDir != ChanDir.COMBINE)
            {
                return CreateNewLine(kLineUnit, currentDir);
            }
            else
            {
                LastKLine.AddKLineUnit(kLineUnit);
                return LastKLine;
            }
        }

        private ChanKLine CreateNewLine(KLineUnit kLineUnit, ChanDir dir)
        {
            var newCombine = new ChanKLine(this.Count, dir);
            newCombine.AddKLineUnit(kLineUnit);

            var lastCombine = LastKLine;
            if (lastCombine != null)
            {
                lastCombine.Next = newCombine;
            }

            newCombine.Pre = lastCombine;

            this.Add(newCombine);
            return newCombine;
        }
    }
}