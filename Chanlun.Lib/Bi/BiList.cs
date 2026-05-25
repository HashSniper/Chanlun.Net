using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Bi
{
    public class BiList : List<Bi>
    {
        private Bi? LastBi => this.IsNotNullOrEmpty() ? this.Last() : null;
        
        public void CreateOrUpdateBiFromKLine(ChanKLine chanKLine)
        {
            if (chanKLine.FX == ChanFX.UNKNOWN)
            {
                return;
            }
            
            if (chanKLine.FX == ChanFX.UNKNOWN)
            {
                return;
            }

            if (LastBi == null)
            {
                TryCreateFirstBi(chanKLine);
            }
            else if (LastBi.EndChanKLine.FX == chanKLine.FX)
            {
                LastBi.UpdateEnd(chanKLine);
            }
            else if (LastBi.EndChanKLine.CanCreateNewBi(chanKLine))
            {
                AddNewBi(LastBi.EndChanKLine, chanKLine);
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
            var newBi = new Bi(this.Count, start, end);
            if (LastBi != null)
            {
                LastBi.Next = newBi;
                newBi.Pre = LastBi;
            }

            this.Add(newBi);
        }
    }
}