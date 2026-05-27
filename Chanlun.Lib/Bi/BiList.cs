using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Bi
{
    public class BiList : List<Bi>
    {
        public void CreateOrUpdateBiFromKLine(ChanKLine chanKLine)
        {
            var lastBi = this.IsNotNullOrEmpty() ? this.Last() : null;

            if (chanKLine.FX == ChanFX.UNKNOWN)
            {
                return;
            }

            if (chanKLine.FX == ChanFX.UNKNOWN)
            {
                return;
            }

            if (lastBi == null)
            {
                TryCreateFirstBi(chanKLine);
            }
            else if (lastBi.EndChanKLine.FX == chanKLine.FX)
            {
                lastBi.TryUpdateEnd(chanKLine);
            }
            else if (lastBi.Pre != null && lastBi.Pre.EndChanKLine.FX == chanKLine.FX)
            {
                if (lastBi.Pre.TryUpdateEnd(chanKLine))
                {
                    lastBi = lastBi.Pre;
                    lastBi.Next = null;
                    this.RemoveEnd();
                }
            }

            if (lastBi != null && lastBi.EndChanKLine.CanCreateNewBi(chanKLine))
            {
                AddNewBi(lastBi.EndChanKLine, chanKLine);
            }
        }

        private bool TryCreateFirstBi(ChanKLine endChanKLine)
        {
            var startKLine = endChanKLine.Pre;
            while (startKLine is { Pre: not null })
            {
                startKLine = startKLine.Pre;
            }

            while (startKLine != null && startKLine != endChanKLine)
            {
                if (startKLine.CanCreateNewBi(endChanKLine))
                {
                    AddNewBi(startKLine, endChanKLine);
                    return true;
                }

                startKLine = startKLine.Next;
            }

            return false;
        }

        private void AddNewBi(ChanKLine start, ChanKLine end)
        {
            var lastBi = this.IsNotNullOrEmpty() ? this.Last() : null;
            var newBi = new Bi(this.Count, start, end);
            if (lastBi != null)
            {
                lastBi.Next = newBi;
                newBi.Pre = lastBi;
            }

            this.Add(newBi);
        }
    }
}