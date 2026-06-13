using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>量能分析指标值</summary>
public class VolumeAnalysisValue
{
    public decimal? VolumeRatio5 { get; set; }
    public decimal? VolumeChangePct { get; set; }
    public decimal? Obv { get; set; }
    public string VolumeSignal { get; set; } = string.Empty;
    public bool? VolumeBullish { get; set; }
}
