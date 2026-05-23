namespace Chan.Lib.Indicators;

public class KdjItem
{
    public double K { get; }
    public double D { get; }
    public double J { get; }

    public KdjItem(double k, double d, double j)
    {
        K = k;
        D = d;
        J = j;
    }
}

public class KDJ
{
    public int Period { get; }
    private readonly List<(double high, double low)> _arr = new();
    private KdjItem _preKdj = new(50, 50, 50);

    public KDJ(int period = 9)
    {
        Period = period;
    }

    public KdjItem Add(double high, double low, double close)
    {
        _arr.Add((high, low));
        if (_arr.Count > Period)
            _arr.RemoveAt(0);

        double hn = _arr.Max(x => x.high);
        double ln = _arr.Min(x => x.low);
        double rsv = hn != ln ? 100 * (close - ln) / (hn - ln) : 0.0;

        double curK = 2.0 / 3.0 * _preKdj.K + 1.0 / 3.0 * rsv;
        double curD = 2.0 / 3.0 * _preKdj.D + 1.0 / 3.0 * curK;
        double curJ = 3 * curK - 2 * curD;

        var cur = new KdjItem(curK, curD, curJ);
        _preKdj = cur;
        return cur;
    }
}
