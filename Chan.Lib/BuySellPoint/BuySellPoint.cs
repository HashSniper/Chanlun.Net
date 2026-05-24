using Chan.Lib.Common;
using Chan.Lib.ChanModel;
using Chan.Lib.Bis;
using Chan.Lib.KLines;

namespace Chan.Lib.BuySellPoints;

public class BuySellPoint
{
    public IChanLine Chan { get; }
    public KLineUnit Klu { get; }
    public bool IsBuy { get; }
    public List<BSP_TYPE> Type { get; }
    public BuySellPoint? RelateBsp1 { get; set; }
    public Features Features { get; }
    public bool IsSegbsp { get; set; } = false;

    public BuySellPoint(IChanLine chan, bool isBuy, BSP_TYPE bsType, BuySellPoint? relateBsp1 = null, Dictionary<string, double>? featureDict = null)
    {
        Chan = chan;
        Klu = chan.GetEndKlu();
        IsBuy = isBuy;
        Type = new List<BSP_TYPE> { bsType };
        RelateBsp1 = relateBsp1;
        Chan.Bsp = this;
        Features = new Features(featureDict);
    }

    public void AddType(BSP_TYPE bsType) => Type.Add(bsType);

    public string Type2str() => string.Join(",", Type.Select(x => x switch
    {
        BSP_TYPE.T1 => "1",
        BSP_TYPE.T1P => "1p",
        BSP_TYPE.T2 => "2",
        BSP_TYPE.T2S => "2s",
        BSP_TYPE.T3A => "3a",
        BSP_TYPE.T3B => "3b",
        _ => ""
    }));

    public void AddAnotherBspProp(BSP_TYPE bsType, BuySellPoint? relateBsp1)
    {
        AddType(bsType);
        if (RelateBsp1 == null)
            RelateBsp1 = relateBsp1;
        else if (relateBsp1 != null)
        {
            if (RelateBsp1.Klu.Idx != relateBsp1.Klu.Idx)
                throw new InvalidOperationException();
        }
    }

    public void AddFeat(object inp1, double? inp2 = null)
    {
        Features.AddFeat(inp1, inp2);
    }
}
