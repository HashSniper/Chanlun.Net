using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Bi
{
    public class Bi(int idx, KLineGroup startKLine, KLineGroup endKLine, bool isSure = true)
        : ChanNode<Bi>(idx)
    {
        
        public ChanDir DIR => StartKLine.FX == ChanFX.TOP ? ChanDir.DOWN : ChanDir.UP;

        public KLineGroup StartKLine { get; set; } = startKLine;

        public KLineGroup EndKLine { get; set; } = endKLine;

        public override float High => DIR.IsUp() ? endKLine.High : startKLine.High;
        public override float Low => DIR.IsUp() ? startKLine.Low : endKLine.Low;

        public bool? IsSure { get; set; } = isSure;

        public List<KLineGroup> SureEndKLines { get; set; } = new();

        public override string ToString() => $"{Idx}|{DIR}";
        
        public void RestoreToSureEnd(KLineGroup end)
        {
            IsSure = true;
            UpdateNewEnd(end);
            SureEndKLines.Clear();
        }

        private void UpdateNewEnd(KLineGroup end)
        {
            EndKLine = end;
        }


        public bool TryUpdateEnd(KLineGroup end)
        {
            if ((DIR == ChanDir.UP && end.DIR == ChanDir.UP && end.High >= EndKLine.High) ||
                (DIR == ChanDir.DOWN && end.DIR == ChanDir.DOWN && end.Low <= EndKLine.Low))
            {
                UpdateNewEnd(end);
                return true;
            }

            return false;
        }

        public void UpdateVirtualEnd(KLineGroup end)
        {
            SureEndKLines.Add(EndKLine);
            UpdateNewEnd(end);
            IsSure = false;
        }

        public float GetEndValue()
        {
            return DIR.IsUp() ? EndKLine.High : EndKLine.Low;
        }
        
        public float GetBeginValue()
        {
            return DIR.IsUp() ? EndKLine.Low : EndKLine.High;
        }
    }
}