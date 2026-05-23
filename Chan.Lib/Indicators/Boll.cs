namespace Chan.Lib.Indicators;

public class BollMetric
{
    public double Theta { get; }
    public double UP { get; }
    public double DOWN { get; }
    public double MID { get; }

    public BollMetric(double ma, double theta)
    {
        Theta = theta == 0 ? 1e-7 : theta;
        UP = ma + 2 * theta;
        DOWN = ma - 2 * theta;
        if (DOWN == 0) DOWN = 1e-7;
        MID = ma;
    }
}

public class BollModel
{
    public int N { get; }
    private readonly List<double> _arr = new();

    public BollModel(int n = 20)
    {
        if (n <= 1) throw new ArgumentException("N must be > 1");
        N = n;
    }

    public BollMetric Add(double value)
    {
        _arr.Add(value);
        if (_arr.Count > N)
            _arr.RemoveAt(0);
        double ma = _arr.Average();
        double theta = Math.Sqrt(_arr.Select(x => (x - ma) * (x - ma)).Average());
        return new BollMetric(ma, theta);
    }
}
