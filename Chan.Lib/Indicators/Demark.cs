using Chan.Lib.Common;

namespace Chan.Lib.Indicators;

public class Kl
{
    public int Idx { get; }
    public double Close { get; }
    public double High { get; }
    public double Low { get; }

    public Kl(int idx, double close, double high, double low)
    {
        Idx = idx;
        Close = close;
        High = high;
        Low = low;
    }

    public double V(bool isClose, CHAN_DIR dir)
    {
        if (isClose) return Close;
        return dir == CHAN_DIR.UP ? High : Low;
    }
}

public class DemarkIndex
{
    public List<DemarkIndexData> Data { get; } = new();

    public void Add(CHAN_DIR dir, string type, int idx, DemarkSetup series)
    {
        Data.Add(new DemarkIndexData { Dir = dir, Type = type, Idx = idx, Series = series });
    }

    public List<DemarkIndexData> GetSetup() => Data.Where(d => d.Type == "setup").ToList();
    public List<DemarkIndexData> GetCountdown() => Data.Where(d => d.Type == "countdown").ToList();

    public void Update(DemarkIndex demarkIndex)
    {
        Data.AddRange(demarkIndex.Data);
    }
}

public class DemarkIndexData
{
    public CHAN_DIR Dir { get; set; }
    public string Type { get; set; } = "";
    public int Idx { get; set; }
    public DemarkSetup? Series { get; set; }
}

public class DemarkCountdown
{
    public CHAN_DIR Dir { get; }
    public List<Kl> KlList { get; }
    public int Idx { get; set; } = 0;
    public double TDSTPeak { get; }
    public bool Finish { get; set; } = false;

    public DemarkCountdown(CHAN_DIR dir, List<Kl> klList, double tdstPeak)
    {
        Dir = dir;
        KlList = new List<Kl>(klList);
        TDSTPeak = tdstPeak;
    }

    public bool Update(Kl kl)
    {
        if (Finish) return false;
        KlList.Add(kl);
        if (KlList.Count <= DemarkEngine.CountdownBias) return false;
        if (Idx == DemarkEngine.MaxCountdown)
        {
            Finish = true;
            return false;
        }
        if ((Dir == CHAN_DIR.DOWN && kl.High > TDSTPeak) || (Dir == CHAN_DIR.UP && kl.Low < TDSTPeak))
        {
            Finish = true;
            return false;
        }
        if (Dir == CHAN_DIR.DOWN && KlList[KlList.Count - 1].Close < KlList[KlList.Count - 1 - DemarkEngine.CountdownBias].V(DemarkEngine.CountdownCmp2close, Dir))
        {
            Idx++;
            return true;
        }
        if (Dir == CHAN_DIR.UP && KlList[KlList.Count - 1].Close > KlList[KlList.Count - 1 - DemarkEngine.CountdownBias].V(DemarkEngine.CountdownCmp2close, Dir))
        {
            Idx++;
            return true;
        }
        return false;
    }
}

public class DemarkSetup
{
    public CHAN_DIR Dir { get; }
    public List<Kl> KlList { get; }
    public Kl PreKl { get; }
    public DemarkCountdown? Countdown { get; set; }
    public bool SetupFinished { get; set; } = false;
    public int Idx { get; set; } = 0;
    public double? TDSTPeak { get; set; }
    public DemarkIndex LastDemarkIndex { get; set; } = new();

    public DemarkSetup(CHAN_DIR dir, List<Kl> klList, Kl preKl)
    {
        Dir = dir;
        KlList = new List<Kl>(klList);
        PreKl = preKl;
        if (KlList.Count != DemarkEngine.SetupBias)
            throw new InvalidOperationException();
    }

    public DemarkIndex Update(Kl kl)
    {
        LastDemarkIndex = new DemarkIndex();
        if (!SetupFinished)
        {
            KlList.Add(kl);
            if (Dir == CHAN_DIR.DOWN)
            {
                if (KlList[KlList.Count - 1].Close < KlList[KlList.Count - 1 - DemarkEngine.SetupBias].V(DemarkEngine.SetupCmp2close, Dir))
                    AddSetup();
                else
                    SetupFinished = true;
            }
            else
            {
                if (KlList[KlList.Count - 1].Close > KlList[KlList.Count - 1 - DemarkEngine.SetupBias].V(DemarkEngine.SetupCmp2close, Dir))
                    AddSetup();
                else
                    SetupFinished = true;
            }
        }
        if (Idx == DemarkEngine.DemarkLen && !SetupFinished && Countdown == null)
        {
            Countdown = new DemarkCountdown(Dir, KlList[..^1].ToList(), CalTDSTPeak());
        }
        if (Countdown != null && Countdown.Update(kl))
        {
            LastDemarkIndex.Add(Dir, "countdown", Countdown.Idx, this);
        }
        return LastDemarkIndex;
    }

    private void AddSetup()
    {
        Idx++;
        LastDemarkIndex.Add(Dir, "setup", Idx, this);
    }

    private double CalTDSTPeak()
    {
        if (KlList.Count != DemarkEngine.SetupBias + DemarkEngine.DemarkLen)
            throw new InvalidOperationException();
        var arr = KlList.Skip(DemarkEngine.SetupBias).Take(DemarkEngine.DemarkLen).ToList();
        if (arr.Count != DemarkEngine.DemarkLen)
            throw new InvalidOperationException();
        double res;
        if (Dir == CHAN_DIR.DOWN)
        {
            res = arr.Max(kl => kl.High);
            if (DemarkEngine.TiaokongSt && arr[0].High < PreKl.Close)
                res = Math.Max(res, PreKl.Close);
        }
        else
        {
            res = arr.Min(kl => kl.Low);
            if (DemarkEngine.TiaokongSt && arr[0].Low > PreKl.Close)
                res = Math.Min(res, PreKl.Close);
        }
        TDSTPeak = res;
        return res;
    }
}

public class DemarkEngine
{
    public static int DemarkLen { get; set; } = 9;
    public static int SetupBias { get; set; } = 4;
    public static int CountdownBias { get; set; } = 2;
    public static int MaxCountdown { get; set; } = 13;
    public static bool TiaokongSt { get; set; } = true;
    public static bool SetupCmp2close { get; set; } = true;
    public static bool CountdownCmp2close { get; set; } = true;

    private readonly List<Kl> _klLst = new();
    private readonly List<DemarkSetup> _series = new();

    public DemarkEngine(
        int demarkLen = 9,
        int setupBias = 4,
        int countdownBias = 2,
        int maxCountdown = 13,
        bool tiaokongSt = true,
        bool setupCmp2close = true,
        bool countdownCmp2close = true
    )
    {
        DemarkLen = demarkLen;
        SetupBias = setupBias;
        CountdownBias = countdownBias;
        MaxCountdown = maxCountdown;
        TiaokongSt = tiaokongSt;
        SetupCmp2close = setupCmp2close;
        CountdownCmp2close = countdownCmp2close;
    }

    public DemarkIndex Update(int idx, double close, double high, double low)
    {
        _klLst.Add(new Kl(idx, close, high, low));
        if (_klLst.Count <= SetupBias + 1)
            return new DemarkIndex();

        if (_klLst[_klLst.Count - 1].Close < _klLst[_klLst.Count - 1 - SetupBias].Close)
        {
            if (!_series.Any(s => s.Dir == CHAN_DIR.DOWN && !s.SetupFinished))
            {
                _series.Add(new DemarkSetup(CHAN_DIR.DOWN, _klLst.Skip(_klLst.Count - SetupBias - 1).Take(SetupBias).ToList(), _klLst[_klLst.Count - SetupBias - 2]));
            }
            foreach (var series in _series)
            {
                if (series.Dir == CHAN_DIR.UP && series.Countdown == null && !series.SetupFinished)
                    series.SetupFinished = true;
            }
        }
        else if (_klLst[_klLst.Count - 1].Close > _klLst[_klLst.Count - 1 - SetupBias].Close)
        {
            if (!_series.Any(s => s.Dir == CHAN_DIR.UP && !s.SetupFinished))
            {
                _series.Add(new DemarkSetup(CHAN_DIR.UP, _klLst.Skip(_klLst.Count - SetupBias - 1).Take(SetupBias).ToList(), _klLst[_klLst.Count - SetupBias - 2]));
            }
            foreach (var series in _series)
            {
                if (series.Dir == CHAN_DIR.DOWN && series.Countdown == null && !series.SetupFinished)
                    series.SetupFinished = true;
            }
        }

        Clear();
        CleanSeriesFromSetupFinish();
        var result = CalResult();
        Clear();
        return result;
    }

    private DemarkIndex CalResult()
    {
        var demarkIndex = new DemarkIndex();
        foreach (var series in _series)
            demarkIndex.Update(series.LastDemarkIndex);
        return demarkIndex;
    }

    private void Clear()
    {
        _series.RemoveAll(s => s.SetupFinished && s.Countdown == null);
        _series.RemoveAll(s => s.Countdown != null && s.Countdown.Finish);
    }

    private void CleanSeriesFromSetupFinish()
    {
        int? finishedSetup = null;
        foreach (var series in _series)
        {
            var demarkIdx = series.Update(_klLst[^1]);
            foreach (var setupIdx in demarkIdx.GetSetup())
            {
                if (setupIdx.Idx == DemarkLen)
                {
                    if (finishedSetup.HasValue)
                        throw new InvalidOperationException();
                    finishedSetup = series.GetHashCode();
                }
            }
        }
        if (finishedSetup.HasValue)
            _series.RemoveAll(s => s.GetHashCode() != finishedSetup.Value);
    }
}
