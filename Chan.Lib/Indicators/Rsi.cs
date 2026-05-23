namespace Chan.Lib.Indicators;

public class RSI
{
    public int Period { get; }
    private readonly List<double> _closeArr = new();
    private readonly List<double> _diff = new();
    private readonly List<double> _up = new();
    private readonly List<double> _down = new();

    public RSI(int period = 14)
    {
        Period = period;
    }

    public double Add(double close)
    {
        _closeArr.Add(close);
        if (_closeArr.Count == 1)
            return 50.0;

        _diff.Add(_closeArr[^1] - _closeArr[^2]);

        if (_diff.Count < Period)
        {
            double upSum = _diff.Where(x => x > 0).Sum();
            double downSum = _diff.Where(x => x < 0).Sum(x => -x);
            _up.Add(upSum / _diff.Count);
            _down.Add(downSum / _diff.Count);
        }
        else
        {
            double upVal = _diff[^1] > 0 ? _diff[^1] : 0.0;
            double downVal = _diff[^1] < 0 ? -_diff[^1] : 0.0;
            _up.Add((_up[^1] * (Period - 1) + upVal) / Period);
            _down.Add((_down[^1] * (Period - 1) + downVal) / Period);
        }

        if (_down[^1] == 0)
            return _up[^1] > 0 ? 100.0 : 0.0;

        double rs = _up[^1] / _down[^1];
        return 100.0 - 100.0 / (1.0 + rs);
    }
}
