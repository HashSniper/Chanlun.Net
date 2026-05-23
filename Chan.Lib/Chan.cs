// using Chan.Lib.Common;
// using Chan.Lib.KLines;
// using Chan.Lib.DataAPI;
// using Chan.Lib.BuySellPoints;
//
// namespace Chan.Lib;
//
// public class Chan
// {
//     public string Code { get; }
//     public string? BeginTime { get; }
//     public string? EndTime { get; }
//     public AUTYPE Autype { get; }
//     public DATA_SRC DataSrc { get; }
//     public List<KL_TYPE> LvList { get; }
//     public ChanConfig Conf { get; }
//
//     public int KlMisalignCnt { get; set; } = 0;
//     public Dictionary<string, List<DateTime>> KlInconsistentDetail { get; } = new();
//     public Dictionary<KL_TYPE, List<IEnumerator<KLineUnit>>> GKlIter { get; } = new();
//     public Dictionary<KL_TYPE, KLineList> KlDatas { get; private set; } = new();
//
//     private KLineUnit?[] _kluCache = Array.Empty<KLineUnit?>();
//     private DateTime[] _kluLastT = Array.Empty<DateTime>();
//
//     public Chan(
//         string code,
//         object? beginTime = null,
//         object? endTime = null,
//         DATA_SRC dataSrc = DATA_SRC.BAO_STOCK,
//         List<KL_TYPE>? lvList = null,
//         ChanConfig? config = null,
//         AUTYPE autype = AUTYPE.QFQ
//     )
//     {
//         Code = code;
//         BeginTime = beginTime is DateTime dt ? dt.ToString("yyyy-MM-dd") : beginTime?.ToString();
//         EndTime = endTime is DateTime dt2 ? dt2.ToString("yyyy-MM-dd") : endTime?.ToString();
//         Autype = autype;
//         DataSrc = dataSrc;
//         LvList = lvList ?? new List<KL_TYPE> { KL_TYPE.K_DAY, KL_TYPE.K_60M };
//         FuncUtil.CheckKltypeOrder(LvList);
//         Conf = config ?? new ChanConfig();
//
//         DoInit();
//
//         if (!Conf.TriggerStep)
//         {
//             foreach (var _ in Load())
//             {
//                 // consume all
//             }
//         }
//     }
//
//     public void DoInit()
//     {
//         KlDatas = new Dictionary<KL_TYPE, KLineList>();
//         foreach (var lv in LvList)
//             KlDatas[lv] = new KLineList(lv, Conf);
//     }
//
//     private IEnumerable<KLineUnit> LoadStockData(CommonStockApi stockapiInstance, KL_TYPE lv)
//     {
//         int idx = 0;
//         foreach (var klu in stockapiInstance.GetKlData())
//         {
//             klu.SetIdx(idx);
//             klu.KlType = lv;
//             yield return klu;
//             idx++;
//         }
//     }
//
//     private IEnumerator<KLineUnit> GetLoadStockIter(Type stockapiCls, KL_TYPE lv)
//     {
//         var instance = (CommonStockApi)Activator.CreateInstance(stockapiCls, Code, lv, BeginTime, EndTime, Autype)!;
//         return LoadStockData(instance, lv).GetEnumerator();
//     }
//
//     public void AddLvIter(object lvIdx, IEnumerator<KLineUnit> iter)
//     {
//         KL_TYPE lv;
//         if (lvIdx is int idx)
//             lv = LvList[idx];
//         else
//             lv = (KL_TYPE)lvIdx;
//         if (!GKlIter.ContainsKey(lv))
//             GKlIter[lv] = new List<IEnumerator<KLineUnit>>();
//         GKlIter[lv].Add(iter);
//     }
//
//     private KLineUnit GetNextLvKlu(object lvIdx)
//     {
//         KL_TYPE lv;
//         if (lvIdx is int idx)
//             lv = LvList[idx];
//         else
//             lv = (KL_TYPE)lvIdx;
//         if (!GKlIter.ContainsKey(lv) || GKlIter[lv].Count == 0)
//             throw new InvalidOperationException();
//         try
//         {
//             if (GKlIter[lv][0].MoveNext())
//                 return GKlIter[lv][0].Current;
//             else
//             {
//                 GKlIter[lv].RemoveAt(0);
//                 return GetNextLvKlu(lv);
//             }
//         }
//         catch
//         {
//             GKlIter[lv].RemoveAt(0);
//             if (GKlIter[lv].Count == 0)
//                 throw;
//             return GetNextLvKlu(lv);
//         }
//     }
//
//     public IEnumerable<Chan> StepLoad()
//     {
//         if (!Conf.TriggerStep) throw new InvalidOperationException("trigger_step must be true");
//         DoInit();
//         bool yielded = false;
//         int idx = 0;
//         foreach (var snapshot in Load(step: true))
//         {
//             if (idx < Conf.SkipStep)
//             {
//                 idx++;
//                 continue;
//             }
//
//             yield return snapshot;
//             yielded = true;
//         }
//
//         if (!yielded)
//             yield return this;
//     }
//
//     public void TriggerLoad(Dictionary<KL_TYPE, List<KLineUnit>> inp)
//     {
//         if (_kluCache.Length == 0)
//             _kluCache = new KLineUnit?[LvList.Count];
//         if (_kluLastT.Length == 0)
//             _kluLastT = Enumerable.Repeat(new DateTime(1980, 1, 1, 0, 0), LvList.Count).ToArray();
//
//         for (int lvIdx = 0; lvIdx < LvList.Count; lvIdx++)
//         {
//             var lv = LvList[lvIdx];
//             if (!inp.ContainsKey(lv))
//             {
//                 if (lvIdx == 0)
//                     throw new ChanException($"最高级别{lv}没有传入数据", ErrCode.NO_DATA);
//                 continue;
//             }
//
//             foreach (var klu in inp[lv])
//                 klu.KlType = lv;
//             AddLvIter(lv, inp[lv].GetEnumerator());
//         }
//
//         foreach (var _ in LoadIterator(0, null, false))
//         {
//             // consume
//         }
//
//         if (!Conf.TriggerStep)
//         {
//             foreach (var lv in LvList)
//                 KlDatas[lv].CalSegAndZs();
//         }
//     }
//
//     private List<IEnumerator<KLineUnit>> InitLvKluIter(Type stockapiCls)
//     {
//         var lvKluIter = new List<IEnumerator<KLineUnit>>();
//         var validLvList = new List<KL_TYPE>();
//         foreach (var lv in LvList)
//         {
//             try
//             {
//                 lvKluIter.Add(GetLoadStockIter(stockapiCls, lv));
//                 validLvList.Add(lv);
//             }
//             catch (ChanException e)
//             {
//                 if (e.ErrCode == ErrCode.SRC_DATA_NOT_FOUND && Conf.AutoSkipIllegalSubLv)
//                 {
//                     if (Conf.PrintWarning)
//                         Console.WriteLine($"[WARNING-{Code}]{lv}级别获取数据失败，跳过");
//                     KlDatas.Remove(lv);
//                     continue;
//                 }
//
//                 throw;
//             }
//         }
//
//         LvList.Clear();
//         LvList.AddRange(validLvList);
//         return lvKluIter;
//     }
//
//     private Type GetStockAPI()
//     {
//         return DataSrc switch
//         {
//             DATA_SRC.BAO_STOCK => typeof(BaoStockApi),
//             DATA_SRC.CcxtApi => typeof(CcxtApi),
//             DATA_SRC.CSV => typeof(CsvApi),
//             DATA_SRC.AKSHARE => typeof(AkshareApi),
//             _ => throw new ChanException("load src type error", ErrCode.SRC_DATA_TYPE_ERR)
//         };
//     }
//
//     public IEnumerable<Chan> Load(bool step = false)
//     {
//         var stockapiCls = GetStockAPI();
//         try
//         {
//             var iters = InitLvKluIter(stockapiCls);
//             for (int i = 0; i < iters.Count; i++)
//                 AddLvIter(i, iters[i]);
//             _kluCache = new KLineUnit?[LvList.Count];
//             _kluLastT = Enumerable.Repeat(new Time(1980, 1, 1, 0, 0), LvList.Count).ToArray();
//
//             foreach (var _ in LoadIterator(0, null, step))
//             {
//                 if (step)
//                     yield return this;
//             }
//
//             if (!step)
//             {
//                 foreach (var lv in LvList)
//                     KlDatas[lv].CalSegAndZs();
//             }
//         }
//         finally
//         {
//             // cleanup if needed
//         }
//
//         if (this[0].Count == 0)
//             throw new ChanException("最高级别没有获得任何数据", ErrCode.NO_DATA);
//     }
//
//     public void SetKluParentRelation(KLineUnit parentKlu, KLineUnit klineUnit, KL_TYPE curLv, int lvIdx)
//     {
//         if (Conf.KlDataCheck && FuncUtil.KltypeLteDay(curLv) && FuncUtil.KltypeLteDay(LvList[lvIdx - 1]))
//             CheckKlConsistent(parentKlu, klineUnit);
//         parentKlu.AddChildren(klineUnit);
//         klineUnit.SetParent(parentKlu);
//     }
//
//     public void AddNewKl(KL_TYPE curLv, KLineUnit klineUnit)
//     {
//         try
//         {
//             KlDatas[curLv].AddSingleKlu(klineUnit);
//         }
//         catch (Exception)
//         {
//             if (Conf.PrintErrTime)
//                 Console.WriteLine($"[ERROR-{Code}]在计算{klineUnit.Time}K线时发生错误!");
//             throw;
//         }
//     }
//
//     public void TrySetKluIdx(int lvIdx, KLineUnit klineUnit)
//     {
//         if (klineUnit.Idx >= 0) return;
//         if (this[lvIdx].Count == 0)
//             klineUnit.SetIdx(0);
//         else
//             klineUnit.SetIdx(this[lvIdx][^1][^1].Idx + 1);
//     }
//
//     private IEnumerable<object?> LoadIterator(int lvIdx, KLineUnit? parentKlu, bool step)
//     {
//         var curLv = LvList[lvIdx];
//         KLineUnit? preKlu = null;
//         if (this[lvIdx].Count > 0 && this[lvIdx][^1].Count > 0)
//             preKlu = this[lvIdx][^1][^1];
//
//         while (true)
//         {
//             KLineUnit klineUnit;
//             if (_kluCache[lvIdx] != null)
//             {
//                 klineUnit = _kluCache[lvIdx]!;
//                 _kluCache[lvIdx] = null;
//             }
//             else
//             {
//                 try
//                 {
//                     klineUnit = GetNextLvKlu(lvIdx);
//                     TrySetKluIdx(lvIdx, klineUnit);
//                     if (!(klineUnit.Time > _kluLastT[lvIdx]))
//                         throw new ChanException($"kline time err, cur={klineUnit.Time}, last={_kluLastT[lvIdx]}",
//                             ErrCode.KL_NOT_MONOTONOUS);
//                     _kluLastT[lvIdx] = klineUnit.Time;
//                 }
//                 catch (InvalidOperationException)
//                 {
//                     break;
//                 }
//             }
//
//             if (parentKlu != null && klineUnit.Time > parentKlu.Time)
//             {
//                 _kluCache[lvIdx] = klineUnit;
//                 break;
//             }
//
//             klineUnit.SetPreKlu(preKlu);
//             preKlu = klineUnit;
//             AddNewKl(curLv, klineUnit);
//             if (parentKlu != null)
//                 SetKluParentRelation(parentKlu, klineUnit, curLv, lvIdx);
//             if (lvIdx != LvList.Count - 1)
//             {
//                 foreach (var _ in LoadIterator(lvIdx + 1, klineUnit, step))
//                 {
//                     // recurse
//                 }
//
//                 CheckKlAlign(klineUnit, lvIdx);
//             }
//
//             if (lvIdx == 0 && step)
//                 yield return null;
//         }
//     }
//
//     private void CheckKlConsistent(KLineUnit parentKlu, KLineUnit subKlu)
//     {
//         if (parentKlu.Time.Year != subKlu.Time.Year ||
//             parentKlu.Time.Month != subKlu.Time.Month ||
//             parentKlu.Time.Day != subKlu.Time.Day)
//         {
//             var key = parentKlu.Time.ToStr();
//             if (!KlInconsistentDetail.ContainsKey(key))
//                 KlInconsistentDetail[key] = new List<Time>();
//             KlInconsistentDetail[key].Add(subKlu.Time);
//             if (Conf.PrintWarning)
//                 Console.WriteLine($"[WARNING-{Code}]父级别时间是{parentKlu.Time}，次级别时间却是{subKlu.Time}");
//             if (KlInconsistentDetail.Count >= Conf.MaxKlInconsistentCnt)
//                 throw new ChanException($"父&子级别K线时间不一致条数超过{Conf.MaxKlInconsistentCnt}!!", ErrCode.KL_TIME_INCONSISTENT);
//         }
//     }
//
//     private void CheckKlAlign(KLineUnit klineUnit, int lvIdx)
//     {
//         if (Conf.KlDataCheck && klineUnit.SubKlList.Count == 0)
//         {
//             KlMisalignCnt++;
//             if (Conf.PrintWarning)
//                 Console.WriteLine($"[WARNING-{Code}]当前{klineUnit.Time}没在次级别{LvList[lvIdx + 1]}找到K线!!");
//             if (KlMisalignCnt >= Conf.MaxKlMisalignCnt)
//                 throw new ChanException($"在次级别找不到K线条数超过{Conf.MaxKlMisalignCnt}!!", ErrCode.KL_DATA_NOT_ALIGN);
//         }
//     }
//
//     public KLineList this[int n] => KlDatas[LvList[n]];
//     public KLineList this[KL_TYPE n] => KlDatas[n];
//
//     public List<BuySellPoint> GetBsp(int? idx = null)
//     {
//         Console.WriteLine("[deprecated] use get_latest_bsp instead");
//         if (idx.HasValue)
//             return this[idx.Value].BsPointLst.GetSortedBspList();
//         if (LvList.Count != 1) throw new InvalidOperationException();
//         return this[0].BsPointLst.GetSortedBspList();
//     }
//
//     public List<BuySellPoint> GetLatestBsp(int? idx = null, int number = 1)
//     {
//         if (idx.HasValue)
//             return this[idx.Value].BsPointLst.GetLatestBsp(number);
//         if (LvList.Count != 1) throw new InvalidOperationException();
//         return this[0].BsPointLst.GetLatestBsp(number);
//     }
// }