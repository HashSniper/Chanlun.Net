namespace Chanlun.Lib.ChanCommon
{
    public abstract class ChanNode<T>(int idx) where T : class
    {
        public int Idx { get; } = idx;
        public T? Pre { get; set; }

        public T? Next { get; set; }

        public virtual decimal High { get; set; }

        public virtual decimal Low { get; set; }

        public override string ToString()
        {
            return $"{Idx}: {typeof(T).Name}";
        }
    }
}