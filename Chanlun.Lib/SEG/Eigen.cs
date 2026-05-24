using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.SEG;

public class Eigen
{
    public List<Bi.Bi> Bis { get; set; } = [];
    public float High =>Bis.IsNullOrEmpty() ? 0 : Bis.Max(b => b.High);
    public float Low => Bis.IsNullOrEmpty() ? 0 : Bis.Min(b => b.Low);
}