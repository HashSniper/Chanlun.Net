namespace TdxQuantNet.Interfaces;

/// <summary>
/// 通用函数
/// </summary>
/// <see href="https://help.tdx.com.cn/quant/docs/markdown/ctx.stock.md/">help.tdx.com.cn</see>
public interface ITdxQuantUniversal
{
    /// <summary>
    /// 
    /// </summary>
    /// <see href="https://help.tdx.com.cn/quant/docs/markdown/ctx.stock.md/mindoc-1h1104d65vr68.html">subscribe_hq</see>
    public void SubscribeHq();

    public void UnsubscribeHq();
}
