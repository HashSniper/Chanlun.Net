namespace Chan.Lib.Indicators;

public class MacdItem
{
    public double FastEma { get; }
    public double SlowEma { get; }
    public double DIF { get; }
    public double DEA { get; }
    public double Macd { get; }

    public MacdItem(double fastEma, double slowEma, double dif, double dea)
    {
        FastEma = fastEma;
        SlowEma = slowEma;
        DIF = dif;
        DEA = dea;
        Macd = 2 * (dif - dea);
    }
}

public class Macd
{
    public List<MacdItem> MacdInfo { get; } = new();
    public int FastPeriod { get; }
    public int SlowPeriod { get; }
    public int SignalPeriod { get; }

    public Macd(int fastperiod = 12, int slowperiod = 26, int signalperiod = 9)
    {
        FastPeriod = fastperiod;
        SlowPeriod = slowperiod;
        SignalPeriod = signalperiod;
    }

    public MacdItem Add(double value)
    {
        if (MacdInfo.Count == 0)
        {
            MacdInfo.Add(new MacdItem(fastEma: value, slowEma: value, dif: 0, dea: 0));
        }
        else
        {
            var last = MacdInfo[^1];
            double fastEma = (2 * value + (FastPeriod - 1) * last.FastEma) / (FastPeriod + 1);
            double slowEma = (2 * value + (SlowPeriod - 1) * last.SlowEma) / (SlowPeriod + 1);
            double dif = fastEma - slowEma;
            double dea = (2 * dif + (SignalPeriod - 1) * last.DEA) / (SignalPeriod + 1);
            MacdInfo.Add(new MacdItem(fastEma, slowEma, dif, dea));
        }
        return MacdInfo[^1];
    }
}
