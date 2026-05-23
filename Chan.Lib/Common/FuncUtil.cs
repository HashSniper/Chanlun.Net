namespace Chan.Lib.Common;

public static class FuncUtil
{
    private static readonly Dictionary<KL_TYPE, int> KlTypeOrder = new()
    {
        { KL_TYPE.K_1M, 1 },
        { KL_TYPE.K_3M, 2 },
        { KL_TYPE.K_5M, 3 },
        { KL_TYPE.K_15M, 4 },
        { KL_TYPE.K_30M, 5 },
        { KL_TYPE.K_60M, 6 },
        { KL_TYPE.K_DAY, 7 },
        { KL_TYPE.K_WEEK, 8 },
        { KL_TYPE.K_MON, 9 },
        { KL_TYPE.K_QUARTER, 10 },
        { KL_TYPE.K_YEAR, 11 }
    };

    public static bool KltypeLtDay(KL_TYPE type)
    {
        return type is KL_TYPE.K_1M or KL_TYPE.K_5M or KL_TYPE.K_15M or KL_TYPE.K_30M or KL_TYPE.K_60M;
    }

    public static bool KltypeLteDay(KL_TYPE type)
    {
        return type is KL_TYPE.K_1M or KL_TYPE.K_5M or KL_TYPE.K_15M or KL_TYPE.K_30M or KL_TYPE.K_60M or KL_TYPE.K_DAY;
    }

    public static void CheckKltypeOrder(List<KL_TYPE> typeList)
    {
        int lastLv = int.MaxValue;
        foreach (var klType in typeList)
        {
            int curLv = KlTypeOrder[klType];
            if (curLv >= lastLv)
                throw new ChanException("lv_list的顺序必须从大级别到小级别", ErrCode.PARA_ERROR);
            lastLv = curLv;
        }
    }

    public static BI_DIR RevertBiDir(BI_DIR dir)
    {
        return dir == BI_DIR.UP ? BI_DIR.DOWN : BI_DIR.UP;
    }

    public static bool HasOverlap(double l1, double h1, double l2, double h2, bool equal = false)
    {
        return equal ? h2 >= l1 && h1 >= l2 : h2 > l1 && h1 > l2;
    }

    public static double Str2Float(string s)
    {
        return double.TryParse(s, out var result) ? result : 0.0;
    }

    public static object ParseInf(object v)
    {
        if (v is double d)
        {
            if (double.IsPositiveInfinity(d))
                return "double.PositiveInfinity";
            if (double.IsNegativeInfinity(d))
                return "double.NegativeInfinity";
        }
        return v;
    }
}
