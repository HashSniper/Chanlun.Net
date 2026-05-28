using Chanlun.Lib.Extensions;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib.Zs;
using Bi= Bi.Bi;

public class BiPivotList : List<BiPivot>
{
    private BiPivot? _currentPivot;
    private int _searchIndex; // 用于扫描新中枢的起点指针

    public void CreateOrUpdatePivot(List<Bi> biList)
    {
        if (biList.IsNullOrEmpty() || biList.Count < 3)
        {
            return;
        }

        while (_searchIndex < biList.Count)
        {
            TryCreatePivot(biList, biList[_searchIndex]);
        }
    }

    private void TryCreatePivot(List<Bi> biList, Bi curBi)
    {
        if (_currentPivot is null || _currentPivot.IsClosed)
        {
            TryInitializeNewPivot(biList);
            return;
        }

        // 当前有运行中的中枢，尝试延伸
        bool isExtended = _currentPivot.ProcessNextSegment(curBi);
        if (isExtended)
        {
            _searchIndex++;
        }
        else
        {
            // 中枢已经闭合，重置搜索指针。
            // 缠论中，当前的退出段往往会成为下一个中枢的进入段（或组成部分）
            _searchIndex = _currentPivot.OutBi.Idx;
            if (_searchIndex == biList.Count - 1)
            {
                _searchIndex = biList.Count; // 增加索引，用于终结循环
            }
        }
    }

    private void TryInitializeNewPivot(List<Bi> biList)
    {
        // 尝试构建新中枢，要求至少有 4 条线段（1个进入段 + 3个重叠段）
        if (biList.Count - _searchIndex < 4)
        {
            _searchIndex = biList.Count;
            return;
        }

        var entry = biList[_searchIndex];
        var s1 = biList[_searchIndex + 1];
        var s2 = biList[_searchIndex + 2];
        var s3 = biList[_searchIndex + 3];

        var newPivot = new BiPivot(Count);
        if (!newPivot.TryInitialize(entry, s1, s2, s3))
        {
            // 无法形成中枢，指针右移一位，继续寻找
            _searchIndex++;
            return;
        }

        _currentPivot = newPivot;
        Add(_currentPivot);

        // 改变指针位置，下一次搜索从 s3 后面开始
        _searchIndex += 4;

        CheckAdjacentPivotExpansion();
    }

    private void CheckAdjacentPivotExpansion()
    {
        if (Count < 2)
        {
            return;
        }

        var previous = this[^2];
        var current = this[^1];

        // 只有同级别中枢才能互相扩展
        if (previous.Level != current.Level)
        {
            return;
        }

        // 缠论规则：相邻同级别中枢的震荡区间 (GG, DD) 重叠，发生中枢扩展
        if (previous.DD <= current.GG && previous.GG >= current.DD)
        {
            // 在实际工程中，这里通常会生成一个新的高级别 Pivot 对象，
            // 并将 previous 和 current 标记为该高级别 Pivot 的子组件。
            // 此处简化为直接将 current 级别提升。
            current.Level = previous.Level + 1;
        }
    }
}