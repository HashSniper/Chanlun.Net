using System.Text.Json.Serialization;

namespace Chanlun.Lib.ChanCommon
{
    public abstract class ChanNode<T>(int idx) where T : class
    {
        public int Idx { get; } = idx;

        [JsonIgnore]
        public T? Pre { get; set; }

        [JsonIgnore]
        public T? Next { get; set; }

        public virtual decimal High { get; set; }

        public virtual decimal Low { get; set; }

        public override string ToString()
        {
            return $"{Idx}: {typeof(T).Name}";
        }
    }
}