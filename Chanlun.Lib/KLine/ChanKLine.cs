using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.KLine
{
    /// <summary>
    /// 合并后的K线数据，包含合并
    /// </summary>
    public class ChanKLine(int idx, ChanDir dir) : ChanNode<ChanKLine>(idx)
    {
        private readonly List<KLineUnit> _combinedUnits = new();

        public ChanDir DIR { get; set; } = dir;

        public KLineUnit PeakUnit { get; private set; }

        public ChanFX FX
        {
            get
            {
                if (field != ChanFX.UNKNOWN || Pre == null || Next == null) return field;
                if (Pre.High < High && Next.High < High && Pre.Low < Low && Next.Low < Low)
                {
                    field = ChanFX.TOP;
                }
                else if (Pre.High > High && Next.High > High && Pre.Low > Low && Next.Low > Low)
                {
                    field = ChanFX.BOTTOM;
                }
                return field;
            }
        }
        
        public IReadOnlyList<KLineUnit> CombinedUnits => _combinedUnits;

        public void AddKLineUnit(KLineUnit unit)
        {
            _combinedUnits.Add(unit);
            High = DIR == ChanDir.UP ? _combinedUnits.Max(p => p.High) : _combinedUnits.Min(p => p.High);
            Low = DIR == ChanDir.UP ? _combinedUnits.Max(p => p.Low) : _combinedUnits.Min(p => p.Low);
            PeakUnit = DIR == ChanDir.UP
                ? _combinedUnits.Find(p => p.High == High)
                : _combinedUnits.Find(p => p.Low == Low);
        }
    }
}