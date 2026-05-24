using Chan.Lib.Common;
using Chan.Lib.Combiner;

namespace Chan.Lib.KLines;

public class KLine(KLineUnit klUnit, int idx, Combiner_DIR dir = Combiner_DIR.UP) : Combiner<KLineUnit>(klUnit, dir)
{
    public int Idx { get; } = idx;
    public KL_TYPE? KlType { get; set; } = klUnit.KlType;

    public override string ToString()
    {
        string fxToken = Fx switch
        {
            FX_TYPE.TOP => "^",
            FX_TYPE.BOTTOM => "_",
            _ => ""
        };
        return $"{Idx}th{fxToken}:{TimeBegin}~{TimeEnd}({KlType}|{Count}) low={Low} high={High}";
    }
    

    public double GetKluMaxHigh() => Lst.Max(x => x.High);
    public double GetKluMinLow() => Lst.Min(x => x.Low);

    public bool HasGapWithNext()
    {
        if (Next == null) throw new InvalidOperationException("next is null");
        var next = (KLine)Next!;
        return !FuncUtil.HasOverlap(GetKluMinLow(), GetKluMaxHigh(), next.GetKluMinLow(), next.GetKluMaxHigh(), equal: true);
    }

    public bool CheckFxValid(KLine item2, FX_CHECK_METHOD method, bool forVirtual = false)
    {
        if (Next == null || item2.Pre == null)
            throw new InvalidOperationException();
        if (Pre == null)
            throw new InvalidOperationException();
        if (item2.Idx <= Idx)
            throw new InvalidOperationException();

        if (Fx == FX_TYPE.TOP)
        {
            if (!forVirtual && item2.Fx != FX_TYPE.BOTTOM)
                throw new InvalidOperationException();
            if (forVirtual && item2.Dir != Combiner_DIR.DOWN)
                return false;

            double item2High, selfLow;
            if (method == FX_CHECK_METHOD.HALF)
            {
                item2High = Math.Max(item2.Pre.High, item2.High);
                selfLow = Math.Min(Low, Next.Low);
            }
            else if (method == FX_CHECK_METHOD.LOSS)
            {
                item2High = item2.High;
                selfLow = Low;
            }
            else if (method == FX_CHECK_METHOD.STRICT || method == FX_CHECK_METHOD.TOTALLY)
            {
                if (forVirtual)
                    item2High = Math.Max(item2.Pre.High, item2.High);
                else
                {
                    if (item2.Next == null) throw new InvalidOperationException();
                    item2High = Math.Max(Math.Max(item2.Pre.High, item2.High), item2.Next.High);
                }
                selfLow = Math.Min(Math.Min(Pre.Low, Low), Next.Low);
            }
            else
            {
                throw new ChanException("bi_fx_check config error!", ErrCode.CONFIG_ERROR);
            }

            return method == FX_CHECK_METHOD.TOTALLY
                ? Low > item2High
                : High > item2High && item2.Low < selfLow;
        }
        else if (Fx == FX_TYPE.BOTTOM)
        {
            if (!forVirtual && item2.Fx != FX_TYPE.TOP)
                throw new InvalidOperationException();
            if (forVirtual && item2.Dir != Combiner_DIR.UP)
                return false;

            double item2Low, curHigh;
            if (method == FX_CHECK_METHOD.HALF)
            {
                item2Low = Math.Min(item2.Pre.Low, item2.Low);
                curHigh = Math.Max(High, Next.High);
            }
            else if (method == FX_CHECK_METHOD.LOSS)
            {
                item2Low = item2.Low;
                curHigh = High;
            }
            else if (method == FX_CHECK_METHOD.STRICT || method == FX_CHECK_METHOD.TOTALLY)
            {
                if (forVirtual)
                    item2Low = Math.Min(item2.Pre.Low, item2.Low);
                else
                {
                    if (item2.Next == null) throw new InvalidOperationException();
                    item2Low = Math.Min(Math.Min(item2.Pre.Low, item2.Low), item2.Next.Low);
                }
                curHigh = Math.Max(Math.Max(Pre.High, High), Next.High);
            }
            else
            {
                throw new ChanException("bi_fx_check config error!", ErrCode.CONFIG_ERROR);
            }

            return method == FX_CHECK_METHOD.TOTALLY
                ? High < item2Low
                : Low < item2Low && item2.High > curHigh;
        }
        else
        {
            throw new ChanException("only top/bottom fx can check_valid_top_button", ErrCode.BI_ERR);
        }
    }
}
