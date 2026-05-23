using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.KLine
{
    public class KLineGroupList : List<KLineGroup>
    {
        private KLineGroup? LastKLine => this.IsNotNullOrEmpty() ? this.Last() : null;

        public KLineGroup CreateOrUpdateKLineCombineFromUnit(KLineUnit kLineUnit)
        {
            if (this.Count == 0)
            {
                return CreateNewCombine(kLineUnit, ChanDir.UP);
            }

            var currentDir = LastKLine.GetNewNodeDir(kLineUnit);

            if (currentDir.HasValue)
            {
                return CreateNewCombine(kLineUnit, currentDir.Value);
            }
            else
            {
                LastKLine.AddKLineUnit(kLineUnit);
                return LastKLine;
            }
        }

        private KLineGroup CreateNewCombine(KLineUnit kLineUnit, ChanDir dir)
        {
            var newCombine = new KLineGroup(this.Count, dir);
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