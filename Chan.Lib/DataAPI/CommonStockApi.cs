using Chan.Lib.Common;
using Chan.Lib.KLines;

namespace Chan.Lib.DataAPI;

public abstract class CommonStockApi
{
    public string Code { get; }
    public string? Name { get; set; }
    public bool? IsStock { get; set; }
    public KL_TYPE KType { get; }
    public string? BeginDate { get; }
    public string? EndDate { get; }
    public AUTYPE Autype { get; }

    public CommonStockApi(string code, KL_TYPE kType, string? beginDate, string? endDate, AUTYPE autype)
    {
        Code = code;
        KType = kType;
        BeginDate = beginDate;
        EndDate = endDate;
        Autype = autype;
        SetBasicInfo();
    }

    public abstract IEnumerable<KLineUnit> GetKlData();
    public abstract void SetBasicInfo();
    public abstract void DoInit();
    public abstract void DoClose();
}
