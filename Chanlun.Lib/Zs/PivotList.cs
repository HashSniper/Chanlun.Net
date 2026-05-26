using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib.Zs;

public class PivotList : List<Pivot>
{
    private Pivot? _currentPivot = null;
    private int _searchIndex = 0; // 用于扫描新中枢的起点指针
    
    public void CreateOrUpdatePivot(List<Seg> segList)
    {
        if (segList.IsNullOrEmpty() || segList.Count < 3)
        {
            return;
        }

        foreach (var seg in segList)
        {
            TryCreatePivot(segList, seg);
        }
    }

    private void TryCreatePivot(List<Seg> segList, Seg curSeg)
    {
        if (_currentPivot == null || _currentPivot.IsClosed)
        {
            // 尝试构建新中枢，要求至少有 4 条线段（1个进入段 + 3个重叠段）
            while (segList.Count - _searchIndex >= 4)
            {
                var entry = segList[_searchIndex];
                var s1 = segList[_searchIndex + 1];
                var s2 = segList[_searchIndex + 2];
                var s3 = segList[_searchIndex + 3];

                Pivot newPivot = new Pivot(Count);
                if (newPivot.TryInitialize(entry, s1, s2, s3))
                {
                    _currentPivot = newPivot;
                    this.Add(_currentPivot);
                    
                    // 改变指针位置，下一次搜索从s3后面开始
                    _searchIndex = _searchIndex + 4; 
                    
                    CheckAdjacentPivotExpansion();
                    break;
                }
                else
                {
                    // 无法形成中枢，指针右移一位，继续寻找
                    _searchIndex++;
                }
            }
        }
        else
        {
            // 当前有运行中的中枢，尝试延伸
            bool isExtended = _currentPivot.ProcessNextSegment(curSeg);
            if (!isExtended)
            {
                // 中枢已经闭合，重置搜索指针。
                // 缠论中，当前的退出段往往会成为下一个中枢的进入段（或组成部分）
                _searchIndex = segList.IndexOf(_currentPivot.OutSeg);
            }
        }
    }
    
    private void CheckAdjacentPivotExpansion()
    {
        if (Count >= 2)
        {
            var p1 = this[^2];
            var p2 = this[^1];

            // 只有同级别中枢才能互相扩展
            if (p1.Level == p2.Level)
            {
                // 缠论规则：相邻同级别中枢的震荡区间 (GG, DD) 重叠，发生中枢扩展
                if (p1.DD <= p2.GG && p1.GG >= p2.DD)
                {
                    // 在实际工程中，这里通常会生成一个新的高级别 Pivot 对象，
                    // 并将 p1 和 p2 标记为该高级别 Pivot 的子组件。
                    // 此处简化为直接将 p2 级别提升。
                    p2.Level = p1.Level + 1;
                }
            }
        }
    }
    
}