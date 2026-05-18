namespace Chanlun.API;

public static class DuanCalculator
{
    public static float[] Duan1(int nCount, float[] pIn, float[] pHigh, float[] pLow)
    {
        float[] pOut = new float[nCount];
        int nState = 0;
        int nLastD = 0;
        int nLastG = 0;
        float fTop0 = 0;
        float fTop1 = 0;
        float fTop2 = 0;
        float fBot0 = 0;
        float fBot1 = 0;
        float fBot2 = 0;

        for (int i = 0; i < nCount; i++)
        {
            if (pIn[i] == 1)
            {
                fTop1 = fTop2;
                fTop2 = pHigh[i];
            }
            else if (pIn[i] == -1)
            {
                fBot1 = fBot2;
                fBot2 = pLow[i];
            }

            if (nState == 0)
            {
                if (pIn[i] == 1)
                {
                    nState = 1;
                    nLastG = i;
                    pOut[nLastG] = 1;
                    fTop0 = 0;
                    fBot0 = 0;
                }
                else if (pIn[i] == -1)
                {
                    nState = -1;
                    nLastD = i;
                    pOut[nLastD] = -1;
                    fTop0 = 0;
                    fBot0 = 0;
                }
            }
            else if (nState == 1)
            {
                if (pIn[i] == 1)
                {
                    if (pHigh[i] > pHigh[nLastG])
                    {
                        pOut[nLastG] = 0;
                        nLastG = i;
                        pOut[nLastG] = 1;
                        fTop0 = 0;
                        fBot0 = 0;
                    }
                }
                else if (pIn[i] == -1)
                {
                    if (pLow[i] < pLow[nLastD])
                    {
                        nState = -1;
                        nLastD = i;
                        pOut[nLastD] = -1;
                        fTop0 = 0;
                        fBot0 = 0;
                    }
                    else if (fTop1 > 0 && fTop2 > 0 && fBot1 > 0 && fBot2 > 0 && fTop2 < fTop1 && fBot2 < fBot1)
                    {
                        nState = -1;
                        nLastD = i;
                        pOut[nLastD] = -1;
                        fTop0 = 0;
                        fBot0 = 0;
                    }
                    else
                    {
                        if (fBot0 == 0)
                            fBot0 = pLow[i];
                        else if (pLow[i] < fBot0)
                        {
                            nState = -1;
                            nLastD = i;
                            pOut[nLastD] = -1;
                            fTop0 = 0;
                            fBot0 = 0;
                        }
                    }
                }
            }
            else if (nState == -1)
            {
                if (pIn[i] == -1)
                {
                    if (pLow[i] < pLow[nLastD])
                    {
                        pOut[nLastD] = 0;
                        nLastD = i;
                        pOut[nLastD] = -1;
                        fTop0 = 0;
                        fBot0 = 0;
                    }
                }
                else if (pIn[i] == 1)
                {
                    if (pHigh[i] > pHigh[nLastG])
                    {
                        nState = 1;
                        nLastG = i;
                        pOut[nLastG] = 1;
                        fTop0 = 0;
                        fBot0 = 0;
                    }
                    else if (fTop1 > 0 && fTop2 > 0 && fBot1 > 0 && fBot2 > 0 && fTop2 > fTop1 && fBot2 > fBot1)
                    {
                        nState = 1;
                        nLastG = i;
                        pOut[nLastG] = 1;
                        fTop0 = 0;
                        fBot0 = 0;
                    }
                    else
                    {
                        if (fTop0 == 0)
                            fTop0 = pHigh[i];
                        else if (pHigh[i] > fTop0)
                        {
                            nState = 1;
                            nLastG = i;
                            pOut[nLastG] = 1;
                            fTop0 = 0;
                            fBot0 = 0;
                        }
                    }
                }
            }
        }
        return pOut;
    }

    public static float[] Duan2(int nCount, float[] pIn, float[] pHigh, float[] pLow)
    {
        float[] pOut = new float[nCount];
        int nState = 0;
        int nLastD = 0;
        int nLastG = 0;
        float fTop1 = 0;
        float fTop2 = 0;
        float fBot1 = 0;
        float fBot2 = 0;

        for (int i = 0; i < nCount; i++)
        {
            if (pIn[i] == 1)
            {
                fTop1 = fTop2;
                fTop2 = pHigh[i];
            }
            else if (pIn[i] == -1)
            {
                fBot1 = fBot2;
                fBot2 = pLow[i];
            }

            if (nState == 0)
            {
                if (pIn[i] == 1)
                {
                    nState = 1;
                    nLastG = i;
                    pOut[nLastG] = 1;
                }
                else if (pIn[i] == -1)
                {
                    nState = -1;
                    nLastD = i;
                    pOut[nLastD] = -1;
                }
            }
            else if (nState == 1)
            {
                if (pIn[i] == 1)
                {
                    if (pHigh[i] > pHigh[nLastG])
                    {
                        pOut[nLastG] = 0;
                        nLastG = i;
                        pOut[nLastG] = 1;
                    }
                }
                else if (pIn[i] == -1)
                {
                    if (pLow[i] < pLow[nLastD])
                    {
                        nState = -1;
                        nLastD = i;
                        pOut[nLastD] = -1;
                    }
                    else if (fTop1 > 0 && fTop2 > 0 && fBot1 > 0 && fBot2 > 0 && fTop2 < fTop1 && fBot2 < fBot1)
                    {
                        nState = -1;
                        nLastD = i;
                        pOut[nLastD] = -1;
                    }
                }
            }
            else if (nState == -1)
            {
                if (pIn[i] == -1)
                {
                    if (pLow[i] < pLow[nLastD])
                    {
                        pOut[nLastD] = 0;
                        nLastD = i;
                        pOut[nLastD] = -1;
                    }
                }
                else if (pIn[i] == 1)
                {
                    if (pHigh[i] > pHigh[nLastG])
                    {
                        nState = 1;
                        nLastG = i;
                        pOut[nLastG] = 1;
                    }
                    else if (fTop1 > 0 && fTop2 > 0 && fBot1 > 0 && fBot2 > 0 && fTop2 > fTop1 && fBot2 > fBot1)
                    {
                        nState = 1;
                        nLastG = i;
                        pOut[nLastG] = 1;
                    }
                }
            }
        }
        return pOut;
    }
}
