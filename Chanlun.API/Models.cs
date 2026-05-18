using System.Runtime.InteropServices;

namespace Chanlun.API;

// 原始K线
public struct KxianRaw
{
    public float Gao;
    public float Di;
}

// 合并后的K线
public struct Kxian
{
    public float Gao;       // K线高
    public float Di;        // K线低
    public int FangXiang;   // K线方向
    public int KaiShi;      // 开始K线坐标
    public int JieShu;      // 结束K线坐标
    public int ZhongJian;
}

// 笔
public struct Bi
{
    public int FangXiang;           // 笔方向
    public int KaiShi;              // 笔起点
    public int JieShu;              // 笔终点
    public float Gao;               // 笔最高价
    public float Di;                // 笔最低价
    public System.Collections.Generic.List<Kxian> KxianList; // 一笔当中的K线
}

// 中枢
public class ZhongShu
{
    public bool BValid;
    public int NTop1, NTop2, NTop3, NBot1, NBot2, NBot3;
    public float FTop1, FTop2, FTop3, FBot1, FBot2, FBot3;
    public int NLines, NStart, NEnd;
    public float FHigh, FLow;
    public int NDirection;
    public int NTerminate;

    public ZhongShu()
    {
        Reset();
    }

    public void Reset()
    {
        BValid = false;
        NTop1 = NTop2 = NTop3 = NBot1 = NBot2 = NBot3 = 0;
        FTop1 = FTop2 = FTop3 = FBot1 = FBot2 = FBot3 = 0;
        NLines = NStart = NEnd = 0;
        FHigh = FLow = 0;
        NDirection = NTerminate = 0;
    }

    public bool PushHigh(int nIndex, float fValue)
    {
        NTop3 = NTop2;
        FTop3 = FTop2;
        NTop2 = NTop1;
        FTop2 = FTop1;
        NTop1 = nIndex;
        FTop1 = fValue;

        if (BValid)
        {
            if (FTop1 < FLow) // 中枢终结
            {
                NTerminate = -1;
                if (NTop2 > NEnd)
                    NEnd = NTop2;
                return true;
            }
            else
            {
                if (NBot1 > NEnd)
                    NEnd = NBot1;
            }
        }
        else
        {
            if (NTop3 > 0 && NTop2 > 0 && NTop1 > 0 && NBot2 > 0 && NBot1 > 0)
            {
                float fTempHigh = FTop1 < FTop2 ? FTop1 : FTop2;
                float fTempLow = FBot1 > FBot2 ? FBot1 : FBot2;
                if (FTop3 > FTop2 && fTempHigh > fTempLow)
                {
                    NDirection = -1; // 向下中枢
                    NStart = NBot2;
                    NEnd = NTop1;
                    FHigh = fTempHigh;
                    FLow = fTempLow;
                    BValid = true;
                }
            }
        }
        return false;
    }

    public bool PushLow(int nIndex, float fValue)
    {
        NBot3 = NBot2;
        FBot3 = FBot2;
        NBot2 = NBot1;
        FBot2 = FBot1;
        NBot1 = nIndex;
        FBot1 = fValue;

        if (BValid)
        {
            if (FBot1 > FHigh) // 中枢终结
            {
                NTerminate = 1;
                if (NBot2 > NEnd)
                    NEnd = NBot2;
                return true;
            }
            else
            {
                if (NTop1 > NEnd)
                    NEnd = NTop1;
            }
        }
        else
        {
            if (NTop2 > 0 && NTop1 > 0 && NBot3 > 0 && NBot2 > 0 && NBot1 > 0)
            {
                float fTempHigh = FTop1 < FTop2 ? FTop1 : FTop2;
                float fTempLow = FBot1 > FBot2 ? FBot1 : FBot2;
                if (FBot3 < FBot2 && fTempHigh > fTempLow)
                {
                    NDirection = 1; // 向上中枢
                    NStart = NTop2;
                    NEnd = NBot1;
                    FHigh = fTempHigh;
                    FLow = fTempLow;
                    BValid = true;
                }
            }
        }
        return false;
    }
}

// 中枢结果
public struct Pivot
{
    public int S;
    public int E;
    public float Zg;
    public float Zd;
    public float Gg;
    public float Dd;
    public float Direction;
    public bool Affirm;
}

// 通达信插件函数信息
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PluginTCalcFuncInfo
{
    public ushort NFuncMark;
    public nint PCallFunc; // 函数指针
}
