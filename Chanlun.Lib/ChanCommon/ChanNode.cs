namespace Chanlun.Lib.ChanCommon
{
    public class ChanNode<T> where T : class
    {
        public T? Pre { get; set; }

        public T? Next { get; set; }

        public virtual float High { get; set; }
        
        public virtual float Low { get; set; }
    }
}