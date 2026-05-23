using Chan.Lib;
using Chan.Lib.Common;

namespace Chan.Lib.Bis;

public class BiConfig
{
    public string BiAlgo { get; }
    public bool IsStrict { get; }
    public FX_CHECK_METHOD BiFxCheck { get; }
    public bool GapAsKl { get; }
    public bool BiEndIsPeak { get; }
    public bool BiAllowSubPeak { get; }

    public BiConfig(
        string biAlgo = "normal",
        bool isStrict = true,
        string biFxCheck = "half",
        bool gapAsKl = true,
        bool biEndIsPeak = true,
        bool biAllowSubPeak = true
    )
    {
        BiAlgo = biAlgo;
        IsStrict = isStrict;
        BiFxCheck = biFxCheck switch
        {
            "strict" => FX_CHECK_METHOD.STRICT,
            "loss" => FX_CHECK_METHOD.LOSS,
            "half" => FX_CHECK_METHOD.HALF,
            "totally" => FX_CHECK_METHOD.TOTALLY,
            _ => throw new ChanException($"unknown bi_fx_check={biFxCheck}", ErrCode.PARA_ERROR)
        };
        GapAsKl = gapAsKl;
        BiEndIsPeak = biEndIsPeak;
        BiAllowSubPeak = biAllowSubPeak;
    }
}
