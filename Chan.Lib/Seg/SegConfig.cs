using Chan.Lib;
using Chan.Lib.Common;

namespace Chan.Lib.Seg;

public class SegmentConfig
{
    public string SegAlgo { get; }
    public LEFT_SEG_METHOD LeftMethod { get; }

    public SegmentConfig(string segAlgo = "chan", string leftMethod = "peak")
    {
        SegAlgo = segAlgo;
        LeftMethod = leftMethod switch
        {
            "peak" => LEFT_SEG_METHOD.PEAK,
            "all" => LEFT_SEG_METHOD.ALL,
            _ => throw new ChanException($"unknown seg left_method = {leftMethod}", ErrCode.PARA_ERROR)
        };
    }
}
