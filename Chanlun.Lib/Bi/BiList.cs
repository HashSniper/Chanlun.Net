using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Bi
{
    public class BiList : List<Bi>
    {
        private Bi? LastBi => this.IsNotNullOrEmpty() ? this.Last() : null;
        
        public bool CreateOrUpdateBiFromKLine(KLineGroup kLine, bool isNewKLine)
        {
            if (kLine.Pre == null)
            {
                return false;
            }

            var hasCreateOrUpdateSureBi = false;

            if (isNewKLine)
            {
                hasCreateOrUpdateSureBi = CreateOrUpdateSureBiFromKLine(kLine.Pre);
            }

            var hasCreateOrUpdateVirtualB = CreateOrUpdateVirtualBiFromKLine(kLine, !isNewKLine);

            return  hasCreateOrUpdateSureBi || hasCreateOrUpdateVirtualB;
        }

        private bool CreateOrUpdateVirtualBiFromKLine(KLineGroup kLine, bool deleteEndBi)
        {
            if (deleteEndBi)
            {
                DeleteVirtualBi();
            }

            if (LastBi == null)
            {
                return false;
            }

            if (LastBi.EndKLine.Idx == kLine.Idx)
            {
                return false;
            }

            if ((LastBi.DIR == ChanDir.UP && kLine.High >= LastBi.EndKLine.High) ||
                (LastBi.DIR == ChanDir.DOWN) && kLine.Low <= LastBi.EndKLine.Low)
            {
                LastBi.UpdateVirtualEnd(kLine);
                return true;
            }

            var tmpKline = kLine;
            while (tmpKline != null && tmpKline.Idx > LastBi.EndKLine.Idx)
            {
                if (tmpKline.CanCreateNewBi(kLine))
                {
                    AddNewBi(tmpKline, kLine);
                    return true;
                }

                tmpKline = tmpKline.Pre;
            }

            return false;
        }

        private bool CreateOrUpdateSureBiFromKLine(KLineGroup kLine)
        {
            var lastUnitIdxOfLastBiIdxTmp = GetLastUnitIdxOfLastBi();
            DeleteVirtualBi();

            if (!kLine.FX.HasValue)
            {
                return lastUnitIdxOfLastBiIdxTmp != GetLastUnitIdxOfLastBi();
            }

            if (LastBi == null)
            {
                return TryCreateFirstBi(kLine);
            }

            if (LastBi.EndKLine.FX == kLine.FX)
            {
                return LastBi.TryUpdateEnd(kLine);
            }

            if (LastBi.EndKLine.CanCreateNewBi(kLine))
            {
                AddNewBi(LastBi.EndKLine, kLine);
                return true;
            }

            return lastUnitIdxOfLastBiIdxTmp != GetLastUnitIdxOfLastBi();
        }

        private bool TryCreateFirstBi(KLineGroup endKLine)
        {
            var startKLine = endKLine.Pre;
            while (startKLine is { Pre: not null })
            {
                startKLine = startKLine.Pre;
            }

            while (startKLine != null && startKLine != endKLine)
            {
                if (startKLine.CanCreateNewBi(endKLine))
                {
                    AddNewBi(startKLine, endKLine);
                    return true;
                }

                startKLine = startKLine.Next;
            }

            return false;
        }

        private void AddNewBi(KLineGroup start, KLineGroup end)
        {
            var newBi = new Bi(this.Count, start, end);
            if (LastBi != null)
            {
                LastBi.Next = newBi;
                newBi.Pre = LastBi;
            }

            this.Add(newBi);
        }

        private void DeleteVirtualBi()
        {
            if (LastBi == null || (LastBi.IsSure.HasValue && LastBi.IsSure.Value))
            {
                return;
            }

            var surEndList = LastBi.SureEndKLines;
            if (!surEndList.IsNullOrEmpty())
            {
                LastBi.RestoreToSureEnd(surEndList[0]);
            }
            else
            {
                this.RemoveEnd();
            }

            if (LastBi != null)
            {
                LastBi.Next = null;
            }
        }

        private int? GetLastUnitIdxOfLastBi()
        {
            return LastBi?.EndKLine.PeakUnit.Idx;
        }
    }
}