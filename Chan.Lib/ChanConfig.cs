using Chan.Lib.Common;
using Chan.Lib.Bis;
using Chan.Lib.BuySellPoints;
using Chan.Lib.Indicators;
using Chan.Lib.Seg;
using Chan.Lib.ZS;

namespace Chan.Lib;

public class ChanConfig
{
    public BiConfig BiConf { get; }
    public SegmentConfig SegConf { get; }
    public PivotConfig ZsConf { get; }
    public BuySellPointConfig BsPointConf { get; private set; }
    public BuySellPointConfig SegBsPointConf { get; private set; }

    public bool TriggerStep { get; }
    public int SkipStep { get; }
    public bool KlDataCheck { get; }
    public int MaxKlMisalignCnt { get; }
    public int MaxKlInconsistentCnt { get; }
    public bool AutoSkipIllegalSubLv { get; }
    public bool PrintWarning { get; }
    public bool PrintErrTime { get; }

    public List<int> MeanMetrics { get; }
    public List<int> TrendMetrics { get; }
    public Dictionary<string, int> MacdConfig { get; }
    public bool CalDemark { get; }
    public bool CalRsi { get; }
    public bool CalKdj { get; }
    public int RsiCycle { get; }
    public int KdjCycle { get; }
    public Dictionary<string, object> DemarkConfig { get; }
    public int BollN { get; }

    public ChanConfig(Dictionary<string, object>? conf = null)
    {
        conf ??= new Dictionary<string, object>();
        var cwc = new ConfigWithCheck(conf);

        BiConf = new BiConfig(
            biAlgo: cwc.Get("bi_algo", "normal")?.ToString()!,
            isStrict: Convert.ToBoolean(cwc.Get("bi_strict", true)!),
            biFxCheck: cwc.Get("bi_fx_check", "strict")?.ToString()!,
            gapAsKl: Convert.ToBoolean(cwc.Get("gap_as_kl", false)!),
            biEndIsPeak: Convert.ToBoolean(cwc.Get("bi_end_is_peak", true)!),
            biAllowSubPeak: Convert.ToBoolean(cwc.Get("bi_allow_sub_peak", true)!)
        );
        SegConf = new SegmentConfig(
            segAlgo: cwc.Get("seg_algo", "chan")?.ToString()!,
            leftMethod: cwc.Get("left_seg_method", "peak")?.ToString()!
        );
        ZsConf = new PivotConfig(
            needCombine: Convert.ToBoolean(cwc.Get("zs_combine", true)!),
            zsCombineMode: cwc.Get("zs_combine_mode", "zs")?.ToString()!,
            oneBiZs: Convert.ToBoolean(cwc.Get("one_bi_zs", false)!),
            zsAlgo: cwc.Get("zs_algo", "normal")?.ToString()!
        );

        TriggerStep = Convert.ToBoolean(cwc.Get("trigger_step", false)!);
        SkipStep = Convert.ToInt32(cwc.Get("skip_step", 0)!);
        KlDataCheck = Convert.ToBoolean(cwc.Get("kl_data_check", true)!);
        MaxKlMisalignCnt = Convert.ToInt32(cwc.Get("max_kl_misalgin_cnt", 2)!);
        MaxKlInconsistentCnt = Convert.ToInt32(cwc.Get("max_kl_inconsistent_cnt", 5)!);
        AutoSkipIllegalSubLv = Convert.ToBoolean(cwc.Get("auto_skip_illegal_sub_lv", false)!);
        PrintWarning = Convert.ToBoolean(cwc.Get("print_warning", true)!);
        PrintErrTime = Convert.ToBoolean(cwc.Get("print_err_time", true)!);

        MeanMetrics = (cwc.Get("mean_metrics", new List<int>()) as List<int>) ?? new List<int>();
        TrendMetrics = (cwc.Get("trend_metrics", new List<int>()) as List<int>) ?? new List<int>();
        MacdConfig = (cwc.Get("macd", new Dictionary<string, int> { ["fast"] = 12, ["slow"] = 26, ["signal"] = 9 }) as Dictionary<string, int>) ?? new Dictionary<string, int> { ["fast"] = 12, ["slow"] = 12, ["signal"] = 9 };
        CalDemark = Convert.ToBoolean(cwc.Get("cal_demark", false)!);
        CalRsi = Convert.ToBoolean(cwc.Get("cal_rsi", false)!);
        CalKdj = Convert.ToBoolean(cwc.Get("cal_kdj", false)!);
        RsiCycle = Convert.ToInt32(cwc.Get("rsi_cycle", 14)!);
        KdjCycle = Convert.ToInt32(cwc.Get("kdj_cycle", 9)!);
        DemarkConfig = (cwc.Get("demark", new Dictionary<string, object>
        {
            ["demark_len"] = 9,
            ["setup_bias"] = 4,
            ["countdown_bias"] = 2,
            ["max_countdown"] = 13,
            ["tiaokong_st"] = true,
            ["setup_cmp2close"] = true,
            ["countdown_cmp2close"] = true
        }) as Dictionary<string, object>)!;
        BollN = Convert.ToInt32(cwc.Get("boll_n", 20)!);

        SetBspConfig(cwc);
        cwc.Check();
    }

    public List<object> GetMetricModel()
    {
        var res = new List<object>
        {
            new Macd(
                fastperiod: MacdConfig["fast"],
                slowperiod: MacdConfig["slow"],
                signalperiod: MacdConfig["signal"]
            )
        };
        foreach (var meanT in MeanMetrics)
            res.Add(new TrendModel(TREND_TYPE.MEAN, meanT));
        foreach (var trendT in TrendMetrics)
        {
            res.Add(new TrendModel(TREND_TYPE.MAX, trendT));
            res.Add(new TrendModel(TREND_TYPE.MIN, trendT));
        }
        res.Add(new BollModel(BollN));
        if (CalDemark)
        {
            res.Add(new DemarkEngine(
                demarkLen: Convert.ToInt32(DemarkConfig["demark_len"]),
                setupBias: Convert.ToInt32(DemarkConfig["setup_bias"]),
                countdownBias: Convert.ToInt32(DemarkConfig["countdown_bias"]),
                maxCountdown: Convert.ToInt32(DemarkConfig["max_countdown"]),
                tiaokongSt: Convert.ToBoolean(DemarkConfig["tiaokong_st"]),
                setupCmp2close: Convert.ToBoolean(DemarkConfig["setup_cmp2close"]),
                countdownCmp2close: Convert.ToBoolean(DemarkConfig["countdown_cmp2close"])
            ));
        }
        if (CalRsi)
            res.Add(new RSI(RsiCycle));
        if (CalKdj)
            res.Add(new KDJ(KdjCycle));
        return res;
    }

    private void SetBspConfig(ConfigWithCheck conf)
    {
        var paraDict = new Dictionary<string, object>
        {
            ["divergence_rate"] = double.PositiveInfinity,
            ["min_zs_cnt"] = 1,
            ["bsp1_only_multibi_zs"] = true,
            ["max_bs2_rate"] = 0.9999,
            ["macd_algo"] = "peak",
            ["bs1_peak"] = true,
            ["bs_type"] = "1,1p,2,2s,3a,3b",
            ["bsp2_follow_1"] = true,
            ["bsp3_follow_1"] = true,
            ["bsp3_peak"] = false,
            ["bsp2s_follow_2"] = false,
            ["max_bsp2s_lv"] = null,
            ["strict_bsp3"] = false,
            ["bsp3a_max_zs_cnt"] = 1
        };

        var args = new Dictionary<string, object>();
        foreach (var kv in paraDict)
            args[kv.Key] = conf.Get(kv.Key, kv.Value)!;

        BsPointConf = new BuySellPointConfig(
            divergenceRate: Convert.ToDouble(args["divergence_rate"]),
            minZsCnt: Convert.ToInt32(args["min_zs_cnt"]),
            bsp1OnlyMultibiZs: Convert.ToBoolean(args["bsp1_only_multibi_zs"]),
            maxBs2Rate: Convert.ToDouble(args["max_bs2_rate"]),
            macdAlgo: args["macd_algo"]?.ToString()!,
            bs1Peak: Convert.ToBoolean(args["bs1_peak"]),
            bsType: args["bs_type"]?.ToString()!,
            bsp2Follow1: Convert.ToBoolean(args["bsp2_follow_1"]),
            bsp3Follow1: Convert.ToBoolean(args["bsp3_follow_1"]),
            bsp3Peak: Convert.ToBoolean(args["bsp3_peak"]),
            bsp2sFollow2: Convert.ToBoolean(args["bsp2s_follow_2"]),
            maxBsp2sLv: args["max_bsp2s_lv"] as int?,
            strictBsp3: Convert.ToBoolean(args["strict_bsp3"]),
            bsp3aMaxZsCnt: Convert.ToInt32(args["bsp3a_max_zs_cnt"])
        );

        SegBsPointConf = new BuySellPointConfig(
            divergenceRate: Convert.ToDouble(args["divergence_rate"]),
            minZsCnt: Convert.ToInt32(args["min_zs_cnt"]),
            bsp1OnlyMultibiZs: Convert.ToBoolean(args["bsp1_only_multibi_zs"]),
            maxBs2Rate: Convert.ToDouble(args["max_bs2_rate"]),
            macdAlgo: args["macd_algo"]?.ToString()!,
            bs1Peak: Convert.ToBoolean(args["bs1_peak"]),
            bsType: args["bs_type"]?.ToString()!,
            bsp2Follow1: Convert.ToBoolean(args["bsp2_follow_1"]),
            bsp3Follow1: Convert.ToBoolean(args["bsp3_follow_1"]),
            bsp3Peak: Convert.ToBoolean(args["bsp3_peak"]),
            bsp2sFollow2: Convert.ToBoolean(args["bsp2s_follow_2"]),
            maxBsp2sLv: args["max_bsp2s_lv"] as int?,
            strictBsp3: Convert.ToBoolean(args["strict_bsp3"]),
            bsp3aMaxZsCnt: Convert.ToInt32(args["bsp3a_max_zs_cnt"])
        );
        SegBsPointConf.BConf.Set("macd_algo", "slope");
        SegBsPointConf.SConf.Set("macd_algo", "slope");
        SegBsPointConf.BConf.Set("bsp1_only_multibi_zs", false);
        SegBsPointConf.SConf.Set("bsp1_only_multibi_zs", false);

        foreach (var (k, v) in conf.Items())
        {
            var val = v;
            if (val is string) val = $"\"{val}\"";
            val = FuncUtil.ParseInf(val);
            if (k.EndsWith("-buy"))
            {
                var prop = k[..^5];
                BsPointConf.BConf.Set(prop, val);
            }
            else if (k.EndsWith("-sell"))
            {
                var prop = k[..^6];
                BsPointConf.SConf.Set(prop, val);
            }
            else if (k.EndsWith("-segbuy"))
            {
                var prop = k[..^7];
                SegBsPointConf.BConf.Set(prop, val);
            }
            else if (k.EndsWith("-segsell"))
            {
                var prop = k[..^8];
                SegBsPointConf.SConf.Set(prop, val);
            }
            else if (k.EndsWith("-seg"))
            {
                var prop = k[..^4];
                SegBsPointConf.BConf.Set(prop, val);
                SegBsPointConf.SConf.Set(prop, val);
            }
            else if (paraDict.ContainsKey(k))
            {
                BsPointConf.BConf.Set(k, val);
                BsPointConf.SConf.Set(k, val);
            }
            else
            {
                throw new ChanException($"unknown para = {k}", ErrCode.PARA_ERROR);
            }
        }
        BsPointConf.BConf.ParseTargetType();
        BsPointConf.SConf.ParseTargetType();
        SegBsPointConf.BConf.ParseTargetType();
        SegBsPointConf.SConf.ParseTargetType();
    }
}

public class ConfigWithCheck
{
    private readonly Dictionary<string, object> _conf;

    public ConfigWithCheck(Dictionary<string, object> conf)
    {
        _conf = new Dictionary<string, object>(conf);
    }

    public object? Get(string k, object? defaultValue = null)
    {
        if (_conf.TryGetValue(k, out var res))
        {
            _conf.Remove(k);
            return res;
        }
        return defaultValue;
    }

    public IEnumerable<(string key, object value)> Items()
    {
        var keys = _conf.Keys.ToList();
        foreach (var k in keys)
        {
            var v = _conf[k];
            _conf.Remove(k);
            yield return (k, v);
        }
    }

    public void Check()
    {
        if (_conf.Count > 0)
            throw new ChanException($"invalid ChanConfig: {string.Join(",", _conf.Keys)}", ErrCode.PARA_ERROR);
    }
}
