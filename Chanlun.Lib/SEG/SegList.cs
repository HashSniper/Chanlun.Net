using Chanlun.Lib.Bi;
using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.SEG;

public class SegList : List<Seg>
{
    private Seg? LastSeg => this.IsNotNullOrEmpty() ? this.Last() : null;

    public void UpdateOrCreateSeg(BiList biList)
    {
        DeleteVirtualSeg();
        CalSegSure(biList, LastSeg == null ? 0 : LastSeg.EndBi.Idx + 1);
    }

    private void DeleteVirtualSeg()
    {
        var tmp = LastSeg;
        while (tmp is { IsSure: false })
        {
            this.RemoveEnd();
            tmp = tmp.Pre;
        }

        //如果确定线段的分形的第三元素包含不确定笔，也需要重新算，不然线段分形元素的高低点可能不对
        if (LastSeg != null && LastSeg.EigenFx.Eigens[^1].BiList.Last().IsSure==false)
        {
            this.RemoveEnd();
        }
    }

    private void CalSegSure(BiList biList, int beginIdx)
    {
        var upEigen = new EigenFx(ChanDir.UP);
        var downEigen = new EigenFx(ChanDir.DOWN);

        var lasSegDir = LastSeg?.Dir;

        for (var i = beginIdx; i < biList.Count; i++)
        {
            var bi = biList[i];
            EigenFx fx_eigen = null;

            //在上升线段中，取得是向下笔作为特征笔.
            if (bi.DIR == ChanDir.DOWN && lasSegDir != ChanDir.UP)
            {
                if (upEigen.AddBi(bi))
                {
                    fx_eigen = upEigen;
                }
            }
            else if (bi.DIR == ChanDir.UP && lasSegDir != ChanDir.DOWN)
            {
                if (downEigen.AddBi(bi))
                {
                    fx_eigen = downEigen;
                }
            }

            //第一条线段
            if (LastSeg == null)
            {
                if (upEigen.Eigens[1] != null && bi.DIR == ChanDir.DOWN)
                {
                    lasSegDir = ChanDir.DOWN;
                    downEigen.Clear();
                }
                else if (downEigen.Eigens[1] != null && bi.DIR == ChanDir.UP)
                {
                    lasSegDir = ChanDir.UP;
                    upEigen.Clear();
                }

                if (upEigen.Eigens[1] == null && lasSegDir == ChanDir.DOWN && bi.DIR == ChanDir.DOWN)
                {
                    lasSegDir = null;
                }
                else if (downEigen.Eigens[1] == null && lasSegDir == ChanDir.UP && bi.DIR == ChanDir.UP)
                {
                    lasSegDir = null;
                }
            }

            if (fx_eigen != null)
            {
                fx_eigen.TreatFxEigen(biList);
            }
        }


    }

}