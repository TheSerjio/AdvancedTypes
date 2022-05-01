namespace AdvancedTypes
{
    public sealed class CacheCollection<T> where T : class
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> All = new();

        private readonly System.Func<T> New;

        public CacheCollection(System.Func<T> Factory) => New = Factory;

        public int Count => All.Count;

        public void Clear() => All.Clear();

        public T Cached
        {
            get
            {
                if (All.TryDequeue(out var q))
                    return q;
                else
                    return New();
            }
            set => All.Enqueue(value);
        }
    }
}