using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.KLines;
using ChanAdapter.Memory;

namespace ChanAdapter;

public static class ChanLunCalculator
{
    public static void Calculate(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var config = GetConfig();
        var key = pKey[0];
        var times = StockTimeCache.Get(key);
        if (times == null || times.Count != nCount)
        {
            return;
        }

        var lineList = new KLineList(KL_TYPE.K_3M, config);

        for (var i = 0; i < nCount; i++)
        {
            var unit = new KLineUnit(times[i], pHigh[i], pLow[i], i);
            lineList.AddSingleKlu(unit);
        }

        KLineListCache.Add(key, lineList);
    }


    private static ChanConfig GetConfig()
    {
        var conf = new Dictionary<string, object>
        {
            { "trigger_step", true },
            { "seg_algo", "chan" },
            { "zs_combine", true },
            { "zs_combine_mode", "zs" },
            { "bi_fx_check", "half" },
            { "bi_allow_sub_peak", true },
            { "print_warning", false },
            { "print_err_time", false },
            { "kl_data_check", false }
        };

        return new ChanConfig(conf);
    }
}