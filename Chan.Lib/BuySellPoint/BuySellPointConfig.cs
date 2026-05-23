using Chan.Lib.Common;

namespace Chan.Lib.BuySellPoints;

public class BuySellPointConfig
{
    public PointConfig BConf { get; }
    public PointConfig SConf { get; }

    public BuySellPointConfig(
        double divergenceRate,
        int minZsCnt,
        bool bsp1OnlyMultibiZs,
        double maxBs2Rate,
        string macdAlgo,
        bool bs1Peak,
        string bsType,
        bool bsp2Follow1,
        bool bsp3Follow1,
        bool bsp3Peak,
        bool bsp2sFollow2,
        int? maxBsp2sLv,
        bool strictBsp3,
        int bsp3aMaxZsCnt
    )
    {
        BConf = new PointConfig(divergenceRate, minZsCnt, bsp1OnlyMultibiZs, maxBs2Rate, macdAlgo, bs1Peak, bsType, bsp2Follow1, bsp3Follow1, bsp3Peak, bsp2sFollow2, maxBsp2sLv, strictBsp3, bsp3aMaxZsCnt);
        SConf = new PointConfig(divergenceRate, minZsCnt, bsp1OnlyMultibiZs, maxBs2Rate, macdAlgo, bs1Peak, bsType, bsp2Follow1, bsp3Follow1, bsp3Peak, bsp2sFollow2, maxBsp2sLv, strictBsp3, bsp3aMaxZsCnt);
    }

    public PointConfig GetBSConfig(bool isBuy) => isBuy ? BConf : SConf;
}

public class PointConfig
{
    public double DivergenceRate { get; set; }
    public int MinZsCnt { get; set; }
    public bool Bsp1OnlyMultibiZs { get; set; }
    public double MaxBs2Rate { get; set; }
    public MACD_ALGO MacdAlgo { get; set; }
    public bool Bs1Peak { get; set; }
    public List<BSP_TYPE> TargetTypes { get; set; } = new();
    public string TmpTargetTypes { get; set; }
    public bool Bsp2Follow1 { get; set; }
    public bool Bsp3Follow1 { get; set; }
    public bool Bsp3Peak { get; set; }
    public bool Bsp2sFollow2 { get; set; }
    public int? MaxBsp2sLv { get; set; }
    public bool StrictBsp3 { get; set; }
    public int Bsp3aMaxZsCnt { get; set; }

    public PointConfig(
        double divergenceRate,
        int minZsCnt,
        bool bsp1OnlyMultibiZs,
        double maxBs2Rate,
        string macdAlgo,
        bool bs1Peak,
        string bsType,
        bool bsp2Follow1,
        bool bsp3Follow1,
        bool bsp3Peak,
        bool bsp2sFollow2,
        int? maxBsp2sLv,
        bool strictBsp3,
        int bsp3aMaxZsCnt
    )
    {
        DivergenceRate = divergenceRate;
        MinZsCnt = minZsCnt;
        Bsp1OnlyMultibiZs = bsp1OnlyMultibiZs;
        MaxBs2Rate = maxBs2Rate;
        SetMacdAlgo(macdAlgo);
        Bs1Peak = bs1Peak;
        TmpTargetTypes = bsType;
        Bsp2Follow1 = bsp2Follow1;
        Bsp3Follow1 = bsp3Follow1;
        Bsp3Peak = bsp3Peak;
        Bsp2sFollow2 = bsp2sFollow2;
        MaxBsp2sLv = maxBsp2sLv;
        StrictBsp3 = strictBsp3;
        Bsp3aMaxZsCnt = bsp3aMaxZsCnt;
    }

    public void ParseTargetType()
    {
        var d = Enum.GetValues<BSP_TYPE>().ToDictionary(x => x switch
        {
            BSP_TYPE.T1 => "1",
            BSP_TYPE.T1P => "1p",
            BSP_TYPE.T2 => "2",
            BSP_TYPE.T2S => "2s",
            BSP_TYPE.T3A => "3a",
            BSP_TYPE.T3B => "3b",
            _ => throw new InvalidOperationException()
        });
        var types = TmpTargetTypes.Split(',').Select(t => t.Trim()).ToList();
        TargetTypes = types.Select(t => d[t]).ToList();
    }

    public void SetMacdAlgo(string macdAlgo)
    {
        var d = new Dictionary<string, MACD_ALGO>
        {
            ["area"] = MACD_ALGO.AREA,
            ["peak"] = MACD_ALGO.PEAK,
            ["full_area"] = MACD_ALGO.FULL_AREA,
            ["diff"] = MACD_ALGO.DIFF,
            ["slope"] = MACD_ALGO.SLOPE,
            ["amp"] = MACD_ALGO.AMP,
            ["amount"] = MACD_ALGO.AMOUNT,
            ["volumn"] = MACD_ALGO.VOLUMN,
            ["amount_avg"] = MACD_ALGO.AMOUNT_AVG,
            ["volumn_avg"] = MACD_ALGO.VOLUMN_AVG,
            ["turnrate_avg"] = MACD_ALGO.TURNRATE_AVG,
            ["rsi"] = MACD_ALGO.RSI
        };
        MacdAlgo = d[macdAlgo];
    }

    public void Set(string k, object v)
    {
        if (k == "macd_algo")
            SetMacdAlgo(v.ToString()!);
        else
        {
            var prop = GetType().GetProperty(k, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop != null)
                prop.SetValue(this, v);
        }
    }
}
