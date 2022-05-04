namespace AdvancedTypes
{
    [System.Diagnostics.DebuggerDisplay("Count = {Count}")]
    public abstract class BaseSortedCollection<TKey, TValue> : System.Collections.Generic.IEnumerable<TValue>
    {
        private readonly System.Collections.Generic.List<TValue> All;

        public BaseSortedCollection(int capacity)
        {
            All = new(capacity);
        }

        /// <summary>
        /// Takes last object from collection, O(1)
        /// </summary>
        /// <returns>last object</returns>
        public TValue TakeOne() => RemoveAt(All.Count - 1);

        public bool IsEmpty => All.Count == 0;

        public int Count => All.Count;

        public TValue this[int i]
        {
            get => All[i];
            protected set => All[i] = value;
        }

        protected abstract int Compare(TValue left, TKey right);

        /// <summary>
        /// Performs binary search, O(log2(n))
        /// </summary>
        /// <param name="key">key to search</param>
        /// <param name="index">if key was found, index of value, otherwise index for inserting</param>
        /// <param name="result">if key was found, value, otherwise, object at <paramref name="index"/>, or last object if key is larger than largest value in collection</param>
        /// <returns>Was key found</returns>
        public bool TryGet(TKey key, out int index, out TValue result)
        {
            switch (All.Count)
            {
                case 0:
                    index = 0;
                    result = default;
                    return false;
                case 1:
                    result = All[0];
                    var c = Compare(result, key);
                    index = 0;
                    if (c < 0)
                    {
                        index = 1;
                        return false;
                    }
                    else
                        return c == 0;

                default:
                    int min = 0;
                    int max = All.Count - 1;
                    index = 0;
                    result = default;
                    while (min <= max)
                    {
                        index = (min + max) / 2;
                        result = All[index];
                        var comp = Compare(result, key);
                        if (comp < 0)
                            min = index + 1;
                        else if (comp == 0)
                            return true;
                        else
                            max = index - 1;
                    }
                    if (index == All.Count - 1)
                        if (Compare(All[index], key) != 0)
                            index++;
                    return false;
            }
        }

        /// <param name="obj">Object to add if key isn`t in collection</param>
        /// <param name="key">Key to search in collection</param>
        /// <param name="exists">If key is in collection, value of key</param>
        /// <returns>Was object added</returns>
        protected bool Add(TValue obj, TKey key, out TValue exists)
        {
            if (TryGet(key, out var i, out exists))
                return false;
            else
            {
                All.Insert(i, obj);
                return true;
            }
        }

        /// <summary>
        /// Removes and returns object at <paramref name="index"/>
        /// </summary>
        /// <returns>Object removed from <paramref name="index"/></returns>
        public TValue RemoveAt(int index)
        {
            var q = All[index];
            All.RemoveAt(index);
            return q;
        }

        public void Clear() => All.Clear();

        protected void Insert(int index, TValue obj) => All.Insert(index, obj);

        public System.Collections.Generic.IEnumerator<TValue> GetEnumerator() => All.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => All.GetEnumerator();
    }

    [System.Obsolete("Not implemented yet")]
    public sealed class SortedCollection<TKey, TValue> : BaseSortedCollection<TKey, TValue> where TValue : IConvertableTo<TKey>
    {
        public SortedCollection(int capacity) : base(capacity) { }

        protected override int Compare(TValue left, TKey right) => left.CompareTo(right);

        public bool Add(TValue obj, out TValue exist) => Add(obj, obj.Converted, out exist);

        public void Replace(TValue obj, out TValue previous, out bool replaced)
        {
            replaced = TryGet(obj.Converted, out var i, out previous);
            if (replaced)
                this[i] = obj;
            else
                Insert(i, obj);
        }
    }

    [System.Obsolete("Not implemented yet")]
    public sealed class SortedCollection<T> : BaseSortedCollection<T, T> where T : System.IComparable<T>
    {
        public SortedCollection(int capacity) : base(capacity) { }

        public bool Add(T obj) => Add(obj, obj, out var _);

        protected override int Compare(T left, T right) => left.CompareTo(right);
    }
}