namespace Chanlun.API;

public static class ZhongShuCalculator
{
    public static List<Pivot> ZS(int nCount, float[] pIn, float[] pHigh, float[] pLow)
    {
        List<Pivot> zhongShuList = new();
        ZhongShu zhongShuOne = new();

        for (int i = 0; i < nCount; i++)
        {
            if (pIn[i] == 1)
            {
                if (zhongShuOne.PushHigh(i, pHigh[i]))
                {
                    bool bValid = true;
                    float fHighValue = 0;
                    int nHighIndex = 0;
                    int nLowIndex = 0;
                    int nLowIndexTemp = 0;
                    int nHighCount = 0;

                    if (zhongShuOne.NDirection == 1 && zhongShuOne.NTerminate == -1)
                    {
                        bValid = false;
                        for (int x = zhongShuOne.NStart; x <= zhongShuOne.NEnd; x++)
                        {
                            if (pIn[x] == 1)
                            {
                                if (nHighCount == 0)
                                {
                                    nHighCount++;
                                    fHighValue = pHigh[x];
                                    nHighIndex = x;
                                }
                                else
                                {
                                    nHighCount++;
                                    if (pHigh[x] >= fHighValue)
                                    {
                                        if (nHighCount > 2)
                                            bValid = true;
                                        fHighValue = pHigh[x];
                                        nHighIndex = x;
                                        nLowIndex = nLowIndexTemp;
                                    }
                                }
                            }
                            else if (pIn[x] == -1)
                            {
                                nLowIndexTemp = x;
                            }
                        }
                        if (bValid)
                            zhongShuOne.NEnd = nLowIndex;
                        i = nHighIndex - 1;
                    }
                    else
                    {
                        i = zhongShuOne.NEnd - 1;
                    }

                    if (bValid)
                    {
                        Pivot pivot = new();
                        pivot.S = zhongShuOne.NStart;
                        pivot.E = zhongShuOne.NEnd;
                        pivot.Zg = zhongShuOne.FHigh;
                        pivot.Zd = zhongShuOne.FLow;
                        pivot.Direction = zhongShuOne.NDirection;
                        pivot.Gg = Max(pHigh, pivot.S + 1, pivot.E);
                        pivot.Dd = Min(pLow, pivot.S + 1, pivot.E);
                        zhongShuList.Add(pivot);
                    }
                    zhongShuOne.Reset();
                }
            }
            else if (pIn[i] == -1)
            {
                if (zhongShuOne.PushLow(i, pLow[i]))
                {
                    bool bValid = true;
                    float fLowValue = 0;
                    int nLowIndex = 0;
                    int nHighIndex = 0;
                    int nHighIndexTemp = 0;
                    int nLowCount = 0;

                    if (zhongShuOne.NDirection == -1 && zhongShuOne.NTerminate == 1)
                    {
                        bValid = false;
                        for (int x = zhongShuOne.NStart; x <= zhongShuOne.NEnd; x++)
                        {
                            if (pIn[x] == -1)
                            {
                                if (nLowCount == 0)
                                {
                                    nLowCount++;
                                    fLowValue = pLow[x];
                                    nLowIndex = x;
                                }
                                else
                                {
                                    nLowCount++;
                                    if (pLow[x] <= fLowValue)
                                    {
                                        if (nLowCount > 2)
                                            bValid = true;
                                        fLowValue = pLow[x];
                                        nLowIndex = x;
                                        nHighIndex = nHighIndexTemp;
                                    }
                                }
                            }
                            else if (pIn[x] == 1)
                            {
                                nHighIndexTemp = x;
                            }
                        }
                        if (bValid)
                            zhongShuOne.NEnd = nHighIndex;
                        i = nLowIndex - 1;
                    }
                    else
                    {
                        i = zhongShuOne.NEnd - 1;
                    }

                    if (bValid)
                    {
                        Pivot pivot = new();
                        pivot.S = zhongShuOne.NStart;
                        pivot.E = zhongShuOne.NEnd;
                        pivot.Zg = zhongShuOne.FHigh;
                        pivot.Zd = zhongShuOne.FLow;
                        pivot.Direction = zhongShuOne.NDirection;
                        pivot.Gg = Max(pHigh, pivot.S + 1, pivot.E);
                        pivot.Dd = Min(pLow, pivot.S + 1, pivot.E);
                        zhongShuList.Add(pivot);
                    }
                    zhongShuOne.Reset();
                }
            }
        }

        if (zhongShuOne.BValid)
        {
            Pivot pivot = new();
            pivot.S = zhongShuOne.NStart;
            pivot.E = zhongShuOne.NEnd;
            pivot.Zg = zhongShuOne.FHigh;
            pivot.Zd = zhongShuOne.FLow;
            pivot.Direction = zhongShuOne.NDirection;
            pivot.Gg = Max(pHigh, pivot.S + 1, pivot.E);
            pivot.Dd = Min(pLow, pivot.S + 1, pivot.E);
            zhongShuList.Add(pivot);
        }

        return zhongShuList;
    }

    private static float Max(float[] arr, int start, int end)
    {
        float max = arr[start];
        for (int i = start + 1; i < end; i++)
        {
            if (arr[i] > max)
                max = arr[i];
        }
        return max;
    }

    private static float Min(float[] arr, int start, int end)
    {
        float min = arr[start];
        for (int i = start + 1; i < end; i++)
        {
            if (arr[i] < min)
                min = arr[i];
        }
        return min;
    }
}
