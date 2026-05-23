using Chan.Lib.Common;
using Chan.Lib.KLines;

namespace Chan.Lib.DataAPI;

public class AkshareApi : CommonStockApi
{
    public AkshareApi(string code, KL_TYPE kType, string? beginDate, string? endDate, AUTYPE autype)
        : base(code, kType, beginDate, endDate, autype)
    {
    }

    public override IEnumerable<KLineUnit> GetKlData()
    {
        throw new NotImplementedException("Akshare API needs to be implemented");
    }

    public override void SetBasicInfo()
    {
    }

    public override void DoInit()
    {
    }

    public override void DoClose()
    {
    }
}
