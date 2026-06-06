using Stock.Data.Entities;

namespace Stock.Service.Interface;

public class GetKLineQuery
{
    public string Symbol { get; set; }
    public DateTime FromTime { get; set; }
    public DateTime ToTime { get; set; }
    public KlineResolution Resolution { get; set; }
}