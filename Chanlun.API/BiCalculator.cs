namespace Chanlun.API;

public static class BiCalculator
{
    public static float[] Bi1(int nCount, float[] pHigh, float[] pLow)
    {
        float[] pOut = new float[nCount];
        KxianChuLi kxianChuLi = new();
        for (int i = 0; i < nCount; i++)
        {
            kxianChuLi.Add(pHigh[i], pLow[i]);
        }
        for (int i = 1; i < kxianChuLi.KxianList.Count; i++)
        {
            if (kxianChuLi.KxianList[i - 1].FangXiang != kxianChuLi.KxianList[i].FangXiang)
            {
                if (kxianChuLi.KxianList[i - 1].FangXiang == 1)
                    pOut[kxianChuLi.KxianList[i - 1].ZhongJian] = 1;
                else if (kxianChuLi.KxianList[i - 1].FangXiang == -1)
                    pOut[kxianChuLi.KxianList[i - 1].ZhongJian] = -1;
            }
        }
        if (kxianChuLi.KxianList.Count > 0)
        {
            var last = kxianChuLi.KxianList[kxianChuLi.KxianList.Count - 1];
            if (last.FangXiang == 1)
                pOut[last.ZhongJian] = 1;
            else if (last.FangXiang == -1)
                pOut[last.ZhongJian] = -1;
        }
        return pOut;
    }

    public static float[] Bi2(int nCount, float[] pHigh, float[] pLow)
    {
        float[] pOut = new float[nCount];
        KxianChuLi kxianChuLi = new();
        for (int i = 0; i < nCount; i++)
        {
            kxianChuLi.Add(pHigh[i], pLow[i]);
        }
        BiChuLi biChuLi = new();
        biChuLi.Handle(kxianChuLi.KxianList);
        foreach (var iter in biChuLi.BiList)
        {
            if (iter.FangXiang == 1)
                pOut[iter.KxianList[iter.KxianList.Count - 1].ZhongJian] = 1;
            else if (iter.FangXiang == -1)
                pOut[iter.KxianList[iter.KxianList.Count - 1].ZhongJian] = -1;
        }
        return pOut;
    }
}
