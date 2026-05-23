namespace Chanlun.Lib.ChanCommon
{
    public class ChanNode<T> where T : class
    {
        public T? Pre { get; set; }

        public T? Next { get; set; }

        public float High { get; set; }
        
        public float Low { get; set; }
    }
}