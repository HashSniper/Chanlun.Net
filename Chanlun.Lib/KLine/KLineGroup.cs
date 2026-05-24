using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.KLine
{
    /// <summary>
    /// 合并后的K线数据，包含合并
    /// </summary>
    public class KLineGroup(int idx, ChanDir dir) : ChanNode<KLineGroup>(idx)
    {
        private readonly List<KLineUnit> _combinedUnits = new();

        public ChanDir DIR { get; set; } = dir;

        public KLineUnit PeakUnit { get; private set; }

        private ChanFX? _fx;

        public ChanFX? FX
        {
            get
            {
                if (!_fx.HasValue && Pre != null && Next != null)
                {
                    if (Pre.High < High && Next.High < High && Pre.Low < Low && Next.Low < Low)
                    {
                        _fx = ChanFX.TOP;
                    }
                    else if (Pre.High > High && Next.High > High && Pre.Low > Low && Next.Low > Low)
                    {
                        _fx = ChanFX.BOTTOM;
                    }
                }

                return _fx;
            }
        }
        
        public List<KLineUnit> CombinedUnits => _combinedUnits;

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