namespace Chanlun.Lib.MACD;

public class MACD
{
    public MACD(float dif, float dea)
    {
        DIF = dif;
        DEA = dea;
        MACDHist = (dif - dea) * 2;
    }

    public float DIF { get; }

    public float DEA { get; }

    public float MACDHist { get; }
}