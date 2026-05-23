using Chan.Lib.Common;
using Chan.Lib.Bis;
using Chan.Lib.Seg;
using Chan.Lib.ZS;

namespace Chan.Lib.BuySellPoints;

public class BuySellPointList
{
    public Dictionary<BSP_TYPE, (List<BuySellPoint> buy, List<BuySellPoint> sell)> BspStoreDict { get; } = new();
    public Dictionary<int, BuySellPoint> BspStoreFlatDict { get; } = new();
    public List<BuySellPoint> Bsp1List { get; } = new();
    public Dictionary<int, BuySellPoint> Bsp1Dict { get; } = new();
    public BuySellPointConfig Config { get; }
    public int LastSurePos { get; set; } = -1;
    public int LastSureSegIdx { get; set; } = 0;

    public BuySellPointList(BuySellPointConfig bsPointConfig)
    {
        Config = bsPointConfig;
    }

    public void StoreAddBsp(BSP_TYPE bspType, BuySellPoint bsp)
    {
        if (!BspStoreDict.ContainsKey(bspType))
            BspStoreDict[bspType] = (new List<BuySellPoint>(), new List<BuySellPoint>());
        var list = bsp.IsBuy ? BspStoreDict[bspType].buy : BspStoreDict[bspType].sell;
        if (list.Count > 0 && list[^1].Bi.Idx >= bsp.Bi.Idx)
            throw new InvalidOperationException($"{bspType}, {bsp.IsBuy} {list[^1].Bi.Idx} {bsp.Bi.Idx}");
        list.Add(bsp);
        BspStoreFlatDict[bsp.Bi.Idx] = bsp;
    }

    public void AddBsp1(BuySellPoint bsp)
    {
        if (Bsp1List.Count > 0 && Bsp1List[^1].Bi.Idx >= bsp.Bi.Idx)
            throw new InvalidOperationException();
        Bsp1List.Add(bsp);
        Bsp1Dict[bsp.Bi.Idx] = bsp;
    }

    public void ClearStoreEnd()
    {
        foreach (var bspList in BspStoreDict.Values)
        {
            foreach (var list in new[] { bspList.buy, bspList.sell })
            {
                while (list.Count > 0)
                {
                    if (list[^1].Bi.GetEndKlu().Idx <= LastSurePos)
                        break;
                    BspStoreFlatDict.Remove(list[^1].Bi.Idx);
                    list[^1].Bi.Bsp = null;
                    list.RemoveAt(list.Count - 1);
                }
            }
        }
    }

    public void ClearBsp1End()
    {
        while (Bsp1List.Count > 0)
        {
            if (Bsp1List[^1].Bi.GetEndKlu().Idx <= LastSurePos)
                break;
            Bsp1Dict.Remove(Bsp1List[^1].Bi.Idx);
            Bsp1List.RemoveAt(Bsp1List.Count - 1);
        }
    }

    public IEnumerable<BuySellPoint> BspIter()
    {
        foreach (var bspList in BspStoreDict.Values)
        {
            foreach (var bsp in bspList.buy) yield return bsp;
            foreach (var bsp in bspList.sell) yield return bsp;
        }
    }

    public IEnumerable<BuySellPoint> BspIterV2()
    {
        var listIndices = new List<(BSP_TYPE type, bool isBuy, int idx)>();
        foreach (var (bspType, bspList) in BspStoreDict)
        {
            if (bspList.buy.Count > 0)
                listIndices.Add((bspType, true, bspList.buy.Count - 1));
            if (bspList.sell.Count > 0)
                listIndices.Add((bspType, false, bspList.sell.Count - 1));
        }

        while (listIndices.Count > 0)
        {
            int maxIdx = -1;
            int maxBiIdx = -1;
            BuySellPoint? maxBsp = null;

            for (int i = 0; i < listIndices.Count; i++)
            {
                var (bspType, isBuy, idx) = listIndices[i];
                if (idx >= 0)
                {
                    var bsp = (isBuy ? BspStoreDict[bspType].buy : BspStoreDict[bspType].sell)[idx];
                    if (bsp.Bi.Idx > maxBiIdx)
                    {
                        maxBiIdx = bsp.Bi.Idx;
                        maxIdx = i;
                        maxBsp = bsp;
                    }
                }
            }

            if (maxBsp == null) yield break;
            yield return maxBsp;

            listIndices[maxIdx] = (listIndices[maxIdx].type, listIndices[maxIdx].isBuy, listIndices[maxIdx].idx - 1);
            if (listIndices[maxIdx].idx < 0)
                listIndices.RemoveAt(maxIdx);
        }
    }

    public int Count => BspStoreFlatDict.Count;

    public void Cal(IReadOnlyList<IBiLine> biList, SegmentListBase segList)
    {
        ClearStoreEnd();
        ClearBsp1End();
        CalSegBs1point(segList, biList);
        CalSegBs2point(segList, biList);
        CalSegBs3point(segList, biList);
        UpdateLastPos(segList);
    }

    public void UpdateLastPos(SegmentListBase segList)
    {
        LastSurePos = -1;
        LastSureSegIdx = 0;
        for (int i = segList.Count - 1; i >= 0; i--)
        {
            var seg = segList[i];
            if (seg.IsSure)
            {
                LastSurePos = seg.EndBi.GetBeginKlu().Idx;
                LastSureSegIdx = seg.Idx;
                return;
            }
        }
    }

    public bool SegNeedCal(Segment seg) => seg.EndBi.GetEndKlu().Idx > LastSurePos;

    public void AddBs(BSP_TYPE bsType, IBiLine bi, BuySellPoint? relateBsp1 = null, bool isTargetBsp = true, Dictionary<string, double>? featureDict = null)
    {
        bool isBuy = bi.IsDown();
        if (BspStoreFlatDict.TryGetValue(bi.Idx, out var existBsp))
        {
            if (existBsp.IsBuy != isBuy) throw new InvalidOperationException();
            existBsp.AddAnotherBspProp(bsType, relateBsp1);
            return;
        }
        var conf = Config.GetBSConfig(isBuy);
        if (!conf.TargetTypes.Contains(bsType))
            isTargetBsp = false;

        if (!isTargetBsp && bsType != BSP_TYPE.T1 && bsType != BSP_TYPE.T1P)
            return;

        var bsp = new BuySellPoint(bi, isBuy, bsType, relateBsp1, featureDict);
        if (isTargetBsp)
            StoreAddBsp(bsType, bsp);
        else
            bsp.Bi.Bsp = null;
        if (bsType == BSP_TYPE.T1 || bsType == BSP_TYPE.T1P)
            AddBsp1(bsp);
    }

    private void CalSegBs1point(SegmentListBase segList, IReadOnlyList<IBiLine> biList)
    {
        for (int i = LastSureSegIdx; i < segList.Count; i++)
        {
            var seg = segList[i];
            if (!SegNeedCal(seg)) continue;
            CalSingleBs1point(seg, biList);
        }
    }

    private void CalSingleBs1point(Segment seg, IReadOnlyList<IBiLine> biList)
    {
        var bspConf = Config.GetBSConfig(seg.IsDown());
        int zsCnt = bspConf.Bsp1OnlyMultibiZs ? seg.GetMultiBiZsCnt() : seg.ZsLst.Count;
        bool isTargetBsp = bspConf.MinZsCnt <= 0 || zsCnt >= bspConf.MinZsCnt;

        if (seg.ZsLst.Count > 0 &&
            !seg.ZsLst[^1].IsOneBiZs() &&
            ((seg.ZsLst[^1].BiOut != null && seg.ZsLst[^1].BiOut.Idx >= seg.EndBi.Idx) || seg.ZsLst[^1].BiLst[^1].Idx >= seg.EndBi.Idx) &&
            seg.EndBi.Idx - seg.ZsLst[^1].GetBiIn().Idx > 2)
        {
            TreatBsp1(seg, bspConf, isTargetBsp);
        }
        else
        {
            TreatPzBsp1(seg, bspConf, biList, isTargetBsp);
        }
    }

    private void TreatBsp1(Segment seg, PointConfig bspConf, bool isTargetBsp)
    {
        var lastZs = seg.ZsLst[^1];
        var (breakPeak, _) = lastZs.OutBiIsPeak(seg.EndBi.Idx);
        if (bspConf.Bs1Peak && !breakPeak)
            isTargetBsp = false;
        var (isDiver, divergenceRate) = lastZs.IsDivergence(bspConf, seg.EndBi);
        if (!isDiver)
            isTargetBsp = false;
        var featureDict = new Dictionary<string, double> { { "divergence_rate", divergenceRate ?? 0 } };
        AddBs(BSP_TYPE.T1, seg.EndBi, isTargetBsp: isTargetBsp, featureDict: featureDict);
    }

    private void TreatPzBsp1(Segment seg, PointConfig bspConf, IReadOnlyList<IBiLine> biList, bool isTargetBsp)
    {
        var lastBi = seg.EndBi;
        if (lastBi.Idx < 2) return;
        var preBi = biList[lastBi.Idx - 2];
        if (lastBi.SegIdx != preBi.SegIdx) return;
        if (lastBi.Dir != seg.Dir) return;
        if (lastBi.IsDown() && lastBi.Low() > preBi.Low()) return;
        if (lastBi.IsUp() && lastBi.High() < preBi.High()) return;
        double inMetric = preBi.CalMacdMetric(bspConf.MacdAlgo, isReverse: false);
        double outMetric = lastBi.CalMacdMetric(bspConf.MacdAlgo, isReverse: true);
        bool isDiver = outMetric <= bspConf.DivergenceRate * inMetric;
        double divergenceRate = outMetric / (inMetric + 1e-7);
        if (!isDiver)
            isTargetBsp = false;
        var featureDict = new Dictionary<string, double> { { "divergence_rate", divergenceRate } };
        AddBs(BSP_TYPE.T1P, lastBi, isTargetBsp: isTargetBsp, featureDict: featureDict);
    }

    private void CalSegBs2point(SegmentListBase segList, IReadOnlyList<IBiLine> biList)
    {
        for (int i = LastSureSegIdx; i < segList.Count; i++)
        {
            var seg = segList[i];
            var config = Config.GetBSConfig(seg.IsDown());
            if (!config.TargetTypes.Contains(BSP_TYPE.T2) && !config.TargetTypes.Contains(BSP_TYPE.T2S))
                continue;
            if (!SegNeedCal(seg)) continue;
            TreatBsp2(seg, segList, biList);
        }
    }

    private void TreatBsp2(Segment seg, SegmentListBase segList, IReadOnlyList<IBiLine> biList)
    {
        IBiLine bsp1Bi;
        BuySellPoint? realBsp1;
        IBiLine breakBi, bsp2Bi;
        PointConfig bspConf;

        if (segList.Count > 1)
        {
            bspConf = Config.GetBSConfig(seg.IsDown());
            bsp1Bi = seg.EndBi;
            realBsp1 = Bsp1Dict.GetValueOrDefault(bsp1Bi.Idx);
            if (bsp1Bi.Idx + 2 >= biList.Count) return;
            breakBi = biList[bsp1Bi.Idx + 1];
            bsp2Bi = biList[bsp1Bi.Idx + 2];
        }
        else
        {
            bspConf = Config.GetBSConfig(seg.IsUp());
            bsp1Bi = null!;
            realBsp1 = null;
            if (biList.Count == 1) return;
            bsp2Bi = biList[1];
            breakBi = biList[0];
        }

        if (bspConf.Bsp2Follow1 && (bsp1Bi == null || !BspStoreFlatDict.ContainsKey(bsp1Bi.Idx)))
            return;

        double retraceRate = bsp2Bi.Amp() / breakBi.Amp();
        bool bsp2Flag = retraceRate <= bspConf.MaxBs2Rate;
        if (bsp2Flag)
            AddBs(BSP_TYPE.T2, bsp2Bi, relateBsp1: realBsp1);
        else if (bspConf.Bsp2sFollow2)
            return;

        if (!Config.GetBSConfig(seg.IsDown()).TargetTypes.Contains(BSP_TYPE.T2S))
            return;
        TreatBsp2s(segList, biList, bsp2Bi, breakBi, realBsp1, bspConf);
    }

    private void TreatBsp2s(SegmentListBase segList, IReadOnlyList<IBiLine> biList, IBiLine bsp2Bi, IBiLine breakBi, BuySellPoint? realBsp1, PointConfig bspConf)
    {
        int bias = 2;
        double? low = null, high = null;
        while (bsp2Bi.Idx + bias < biList.Count)
        {
            var bsp2sBi = biList[bsp2Bi.Idx + bias];
            if (bspConf.MaxBsp2sLv.HasValue && bias / 2 > bspConf.MaxBsp2sLv.Value)
                break;
            if (bsp2sBi.SegIdx != bsp2Bi.SegIdx &&
                (bsp2sBi.SegIdx < segList.Count - 1 || bsp2sBi.SegIdx - bsp2Bi.SegIdx >= 2 || segList[bsp2Bi.SegIdx!.Value].IsSure))
                break;
            if (bias == 2)
            {
                if (!FuncUtil.HasOverlap(bsp2Bi.Low(), bsp2Bi.High(), bsp2sBi.Low(), bsp2sBi.High()))
                    break;
                low = Math.Max(bsp2Bi.Low(), bsp2sBi.Low());
                high = Math.Min(bsp2Bi.High(), bsp2sBi.High());
            }
            else if (!FuncUtil.HasOverlap(low!.Value, high!.Value, bsp2sBi.Low(), bsp2sBi.High()))
                break;

            if (Bsp2sBreakBsp1(bsp2sBi, breakBi))
                break;
            double retraceRate = Math.Abs(bsp2sBi.GetEndVal() - breakBi.GetEndVal()) / breakBi.Amp();
            if (retraceRate > bspConf.MaxBs2Rate)
                break;

            AddBs(BSP_TYPE.T2S, bsp2sBi, relateBsp1: realBsp1);
            bias += 2;
        }
    }

    private void CalSegBs3point(SegmentListBase segList, IReadOnlyList<IBiLine> biList)
    {
        for (int i = LastSureSegIdx; i < segList.Count; i++)
        {
            var seg = segList[i];
            if (!SegNeedCal(seg)) continue;
            var config = Config.GetBSConfig(seg.IsDown());
            if (!config.TargetTypes.Contains(BSP_TYPE.T3A) && !config.TargetTypes.Contains(BSP_TYPE.T3B))
                continue;

            IBiLine? bsp1Bi;
            BuySellPoint? realBsp1;
            int nextSegIdx, bsp1BiIdx;
            Segment? nextSeg;
            PointConfig bspConf;

            if (segList.Count > 1)
            {
                bsp1Bi = seg.EndBi;
                bsp1BiIdx = bsp1Bi.Idx;
                bspConf = Config.GetBSConfig(seg.IsDown());
                realBsp1 = Bsp1Dict.GetValueOrDefault(bsp1Bi.Idx);
                nextSegIdx = seg.Idx + 1;
                nextSeg = seg.Next;
            }
            else
            {
                nextSeg = seg;
                nextSegIdx = seg.Idx;
                bsp1Bi = null;
                realBsp1 = null;
                bsp1BiIdx = -1;
                bspConf = Config.GetBSConfig(seg.IsUp());
            }

            if (bspConf.Bsp3Follow1 && (bsp1Bi == null || !BspStoreFlatDict.ContainsKey(bsp1Bi.Idx)))
                continue;
            if (nextSeg != null)
                TreatBsp3After(segList, nextSeg, bspConf, biList, realBsp1, bsp1BiIdx, nextSegIdx);
            TreatBsp3Before(segList, seg, nextSeg, bsp1Bi, bspConf, biList, realBsp1, nextSegIdx);
        }
    }

    private void TreatBsp3After(SegmentListBase segList, Segment nextSeg, PointConfig bspConf, IReadOnlyList<IBiLine> biList, BuySellPoint? realBsp1, int bsp1BiIdx, int nextSegIdx)
    {
        var firstZs = nextSeg.GetFirstMultiBiZs();
        if (firstZs == null) return;
        if (bspConf.StrictBsp3 && firstZs.GetBiIn().Idx != bsp1BiIdx + 1)
            return;

        var config = Config.GetBSConfig(nextSeg.IsDown());
        int bsp3aMaxZsCnt = config.Bsp3aMaxZsCnt;
        var multiBiZsLst = nextSeg.GetMultiBiZsLst();
        for (int zsIdx = 0; zsIdx < multiBiZsLst.Count && zsIdx < bsp3aMaxZsCnt; zsIdx++)
        {
            var zs = multiBiZsLst[zsIdx];
            if (zs.BiOut == null || zs.BiOut.Idx + 1 >= biList.Count)
                break;
            var bsp3Bi = biList[zs.BiOut.Idx + 1];
            if (bsp3Bi.ParentSeg == null)
            {
                if (nextSeg.Idx != segList.Count - 1)
                    break;
            }
            else if (bsp3Bi.ParentSeg.Idx != nextSeg.Idx)
            {
                if (bsp3Bi.ParentSeg.BiList.Count >= 3)
                    break;
            }
            if (bsp3Bi.Dir == nextSeg.Dir)
                break;
            if (bsp3Bi.SegIdx != nextSegIdx && nextSegIdx < segList.Count - 2)
                break;
            if (Bsp3Back2zs(bsp3Bi, zs))
                continue;
            bool bsp3PeakZs = Bsp3BreakZspeak(bsp3Bi, zs);
            if (bspConf.Bsp3Peak && !bsp3PeakZs)
                continue;
            AddBs(BSP_TYPE.T3A, bsp3Bi, relateBsp1: realBsp1);
        }
    }

    private void TreatBsp3Before(SegmentListBase segList, Segment seg, Segment? nextSeg, IBiLine? bsp1Bi, PointConfig bspConf, IReadOnlyList<IBiLine> biList, BuySellPoint? realBsp1, int nextSegIdx)
    {
        var cmpZs = seg.GetFinalMultiBiZs();
        if (cmpZs == null) return;
        if (bsp1Bi == null) return;
        if (bspConf.StrictBsp3 && (cmpZs.BiOut == null || cmpZs.BiOut.Idx != bsp1Bi.Idx))
            return;
        int endBiIdx = CalBsp3BiEndIdx(nextSeg);
        for (int i = bsp1Bi.Idx + 2; i < biList.Count; i += 2)
        {
            var bsp3Bi = biList[i];
            if (bsp3Bi.Idx > endBiIdx)
                break;
            if (bsp3Bi.SegIdx != nextSegIdx && bsp3Bi.SegIdx < segList.Count - 1)
                break;
            if (Bsp3Back2zs(bsp3Bi, cmpZs))
                continue;
            AddBs(BSP_TYPE.T3B, bsp3Bi, relateBsp1: realBsp1);
            break;
        }
    }

    public List<BuySellPoint> GetSortedBspList()
    {
        return BspIter().OrderBy(bsp => bsp.Bi.Idx).ToList();
    }

    public List<BuySellPoint> GetLatestBsp(int number)
    {
        var res = new List<BuySellPoint>();
        foreach (var bsp in BspIterV2())
        {
            res.Add(bsp);
            if (number != 0 && res.Count >= number)
                break;
        }
        return res;
    }

    private static bool Bsp2sBreakBsp1(IBiLine bsp2sBi, IBiLine bsp2BreakBi)
    {
        return (bsp2sBi.IsDown() && bsp2sBi.Low() < bsp2BreakBi.Low()) ||
               (bsp2sBi.IsUp() && bsp2sBi.High() > bsp2BreakBi.High());
    }

    private static bool Bsp3Back2zs(IBiLine bsp3Bi, Pivot zs)
    {
        return (bsp3Bi.IsDown() && bsp3Bi.Low() < zs.High) || (bsp3Bi.IsUp() && bsp3Bi.High() > zs.Low);
    }

    private static bool Bsp3BreakZspeak(IBiLine bsp3Bi, Pivot zs)
    {
        return (bsp3Bi.IsDown() && bsp3Bi.High() >= zs.PeakHigh) || (bsp3Bi.IsUp() && bsp3Bi.Low() <= zs.PeakLow);
    }

    private static int CalBsp3BiEndIdx(Segment? seg)
    {
        if (seg == null) return int.MaxValue;
        if (seg.GetMultiBiZsCnt() == 0 && seg.Next == null)
            return int.MaxValue;
        int endBiIdx = seg.EndBi.Idx - 1;
        foreach (var zs in seg.ZsLst)
        {
            if (zs.IsOneBiZs()) continue;
            if (zs.BiOut != null)
            {
                endBiIdx = zs.BiOut.Idx;
                break;
            }
        }
        return endBiIdx;
    }
}
