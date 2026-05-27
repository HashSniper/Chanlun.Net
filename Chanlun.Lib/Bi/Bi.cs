using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Bi
{
    public class Bi(int idx, ChanKLine startChanKLine, ChanKLine endChanKLine)
        : ChanNode<Bi>(idx)
    {
        
        public ChanDir DIR => StartChanKLine.FX == ChanFX.TOP ? ChanDir.DOWN : ChanDir.UP;

        public ChanKLine StartChanKLine { get; set; } = startChanKLine;

        public ChanKLine EndChanKLine { get; set; } = endChanKLine;

        public override float High => DIR.IsUp() ? EndChanKLine.High : StartChanKLine.High;
        public override float Low => DIR.IsUp() ? StartChanKLine.Low : EndChanKLine.Low;

        public override string ToString() => $"{Idx}|{DIR}";
        
        private void UpdateNewEnd(ChanKLine end)
        {
            EndChanKLine = end;
        }

        public bool TryUpdateEnd(ChanKLine end)
        {
            if ((DIR.IsUp() && end.DIR.IsUp() && end.High >= EndChanKLine.High) ||
                (DIR.IsDown() && end.DIR.IsDown() && end.Low <= EndChanKLine.Low))
            {
                UpdateNewEnd(end);
                return true;
            }
            return false;
        }
        

        public float GetEndValue()
        {
            return DIR.IsUp() ? EndChanKLine.High : EndChanKLine.Low;
        }
        
        public float GetBeginValue()
        {
            return DIR.IsUp() ? EndChanKLine.Low : EndChanKLine.High;
        }
    }
}