namespace Chanlun.Lib.MACD;

public class MACD
{
    public MACD(float dif, float dea)
    {
        DIF = dif;
        DEA = dea;
        Hist = (dif - dea) * 2;
    }

    public float DIF { get; }

    public float DEA { get; }

    public float Hist { get; }
}