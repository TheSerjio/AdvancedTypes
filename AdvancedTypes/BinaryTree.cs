using System.Collections.Generic;

namespace AdvancedTypes
{
    public sealed class BinaryTree<T> : ICollection<T> where T : System.IComparable<T>
    {
        public static void ClearCache(bool fast) => Node.ClearCache(fast);

        private readonly List<Node> ResultStackFromGet = new();

        [System.Diagnostics.DebuggerDisplay("Node:{value}")]
        private sealed class Node
        {
            private Node() { }

            public T value;
            public Node small, big;

            public IEnumerable<Node> Enumerate()
            {
                yield return this;
                if (small != null)
                    foreach (var q in small.Enumerate())
                        yield return q;
                if (big != null)
                    foreach (var q in big.Enumerate())
                        yield return q;
            }

            private static readonly object Locker = new();

            private static Node StaticCacheNode;

            public static Node GetFromCache(T with)
            {
                lock (Locker)
                {
                    if (StaticCacheNode == null) return new Node { value = with };

                    var n = StaticCacheNode;
                    StaticCacheNode = n.big;
                    n.big = null;

                    n.value = with;
                    return n;
                }
            }

            public void AddToCache()
            {
                lock (Locker)
                {
                    value = default;
                    small = null;
                    big = StaticCacheNode;
                    StaticCacheNode = this;
                }
            }

            public static void ClearCache(bool fast)
            {
                if (fast)
                {
                    StaticCacheNode = null;
                    return;
                }

                var n = StaticCacheNode;
                StaticCacheNode = null;
                while (n != null)
                {
                    var next = n.big;
                    n.big = null;
                    n = next;
                }
            }
        }

        private Node Root;

        public bool IsEmpty => Root == null;

        private int _count;

        public int Count => _count;

        bool ICollection<T>.IsReadOnly => false;

        private Node Get(T key, bool fill, out int lastComp)
        {
            Node current = Root;
            if (fill)
                ResultStackFromGet.Clear();

            while (true)
            {
                ResultStackFromGet.Add(current);
                lastComp = current.value.CompareTo(key);
                if (lastComp > 0)
                {
                    if (current.small == null)
                        return current;
                    current = current.small;
                }
                else if (lastComp == 0)
                    return current;
                else
                {
                    if (current.big == null)
                        return current;
                    current = current.big;
                }
            }
        }

        private void NotEnumerate()
        {
            if (EnumeratingCount != 0)
                throw new System.InvalidOperationException("Binary tree cannot be changed while enumerating");
        }

        [System.Obsolete("No parent resetting", true)]
        public T TakeOne()
        {
            NotEnumerate();

            if (Root == null)
                throw new System.InvalidOperationException("Cannot take object from empty tree");

            _count -= 1;

            if (Root.big == null && Root.small == null)
            {
                var q = Root.value;
                Root.AddToCache();
                Root = null;
                return q;
            }

            Node previous = null;
            var current = Root;

            while (true)
            {
                var small = current.small;
                var big = current.big;

                if (small == null)
                {
                    if (big == null)
                    {
                        if (current == previous.small)
                            previous.small = null;
                        else
                            previous.big = null;
                        var q = current.value;
                        current.AddToCache();
                        return q;
                    }
                    else
                    {
                        previous = current;
                        current = current.big;
                    }
                }
                else
                {
                    previous = current;
                    current = current.small;
                }
            }
        }

        /// <inheritdoc cref="Add(T)"/>
        public InsertionResult Add(T obj, out T previous, bool replace)
        {
            NotEnumerate();

            if (Root == null)
            {
                previous = default;
                Root = Node.GetFromCache(obj);
                _count += 1;
                return InsertionResult.Added;
            }

            var q = Get(obj, false, out var c);

            previous = q.value;

            if (c == 0)
                if (replace)
                {
                    q.value = obj;
                    return InsertionResult.Replaced;
                }
                else
                    return InsertionResult.Nothing;
            else
            {
                Node next;
                next = Node.GetFromCache(obj);
                _count += 1;
                if (c > 0)
                    q.small = next;
                else
                    q.big = next;
                return InsertionResult.Added;
            }
        }

        public bool Find(T key, out T result)
        {
            if (Root == null)
            {
                result = default;
                return false;
            }
            var q = Get(key, false, out var comp);
            result = q.value;
            return comp == 0;
        }

        private uint EnumeratingCount;

        public IEnumerator<T> GetEnumerator()
        {
            System.Threading.Interlocked.Increment(ref EnumeratingCount);
            try
            {
                if (Root != null)
                    foreach (var q in Root.Enumerate())
                        yield return q.value;
            }
            finally
            {
                System.Threading.Interlocked.Decrement(ref EnumeratingCount);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public void Clear()
        {
            NotEnumerate();

            _count = 0;

            static void Clear(Node node)
            {
                if (node.small != null)
                    Clear(node.small);
                if (node.big != null)
                    Clear(node.big);
                node.AddToCache();
            }

            if (Root != null)
                Clear(Root);

            Root = null;
        }

        public bool Contains(T item) => Find(item, out var _);

        /// <summary>
        /// Why do you use <see cref="ICollection{T}.CopyTo(T[], int)"/> for binary tree?
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var obj in this)
                array[arrayIndex++] = obj;
        }

        /// <summary>
        /// <inheritdoc/><para>Calls <see cref="Remove(T, out T)"/></para>
        /// </summary>
        /// <param name="item"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Remove(T item) => Remove(item, out var _);

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        /// <para>But you also get a <paramref name="previous"/> object that was equal to <paramref name="item"/>.</para>
        /// </summary>
        /// <param name="item"><inheritdoc cref="ICollection{T}.Remove(T)"/></param>
        /// <returns><inheritdoc cref="ICollection{T}.Remove(T)"/></returns>
        public bool Remove(T item, out T previous)
        {
            NotEnumerate();

            if (Root == null)
            {
                previous = default;
                return false;
            }

            var some = Get(item, true, out int lastComparsion);

            if (lastComparsion != 0)
            {
                previous = default;
                return false;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        void ICollection<T>.Add(T item) => throw new System.Runtime.AmbiguousImplementationException();
    }

    /// <summary>
    /// Returned by <see cref="BinaryTree{T}.Add(T, out T, bool)"/>
    /// </summary>
    public enum InsertionResult : byte
    {
        /// <summary>
        /// obj was in tree, but was not replaced because flag was <c>false</c>
        /// </summary>
        Nothing = 1,
        /// <summary>
        /// obj was not in tree, and was added
        /// </summary>
        Added = 2,
        /// <summary>
        /// obj was in tree, and was replaced because flag was <c>true</c>
        /// </summary>
        Replaced = 3
    }
}