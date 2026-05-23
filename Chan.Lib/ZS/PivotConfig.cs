namespace Chan.Lib.ZS;

public class PivotConfig
{
    public bool NeedCombine { get; }
    public string ZsCombineMode { get; }
    public bool OneBiZs { get; }
    public string ZsAlgo { get; }

    public PivotConfig(bool needCombine = true, string zsCombineMode = "zs", bool oneBiZs = false, string zsAlgo = "normal")
    {
        NeedCombine = needCombine;
        ZsCombineMode = zsCombineMode;
        OneBiZs = oneBiZs;
        ZsAlgo = zsAlgo;
    }
}
