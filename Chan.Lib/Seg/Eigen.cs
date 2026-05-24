using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.Bis;

namespace Chan.Lib.Seg;

public class Eigen : Combiner<IChanLine>
{
    public bool Gap { get; set; } = false;

    public Eigen(IChanLine chan, Combiner_DIR dir) : base(chan, dir)
    {
    }

    public void UpdateFx(Eigen pre, Eigen next, bool excludeIncluded = false, int? allowTopEqual = null)
    {
        base.UpdateFx(pre, next, excludeIncluded, allowTopEqual);
        if ((Fx == FX_TYPE.TOP && pre.High < Low) || (Fx == FX_TYPE.BOTTOM && pre.Low > High))
            Gap = true;
    }

    public override string ToString() => $"{this[0].Idx}~{this[^1].Idx} gap={Gap} fx={Fx}";

    public int GetPeakBiIdx()
    {
        if (Fx == FX_TYPE.UNKNOWN) throw new InvalidOperationException();
        var biDir = this[0].Dir;
        if (biDir == CHAN_DIR.UP)
            return GetLowPeakKlu().Idx - 1;
        else
            return GetHighPeakKlu().Idx - 1;
    }
}
