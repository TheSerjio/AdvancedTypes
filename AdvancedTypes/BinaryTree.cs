namespace AdvancedTypes
{
    public sealed class BinaryTree<TKey, TValue> : System.Collections.Generic.IEnumerable<TValue> where TKey : struct where TValue : System.IComparable<TKey>
    {
        private static readonly object Locker = new();

        private static class Cache
        {
            public static Node node;

            public static Node Get(TValue with)
            {
                if (node == null) return new Node { value = with };

                var q = node;
                q.value = with;
                node = q.small;
                q.small = null;
                return q;
            }

            public static void Add(Node n)
            {
                n.value = default;
                n.big = null;
                n.small = node;
                node = n;
            }
        }

        [System.Diagnostics.DebuggerDisplay("Node:{value}")]
        private sealed class Node
        {
            public TValue value;
            public Node small, big;

            public System.Collections.Generic.IEnumerable<Node> Enumerate()
            {
                yield return this;
                if (small != null)
                    foreach (var q in small.Enumerate())
                        yield return q;
                if (big != null)
                    foreach (var q in big.Enumerate())
                        yield return q;
            }
        }

        private Node Root;

        public TValue TakeOne()
        {
            if (Root == null)
                throw new System.InvalidOperationException("Cannot take object from empty tree");

            if (Root.big == null && Root.small == null)
            {
                var q = Root.value;
                Cache.Add(Root);
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
                        Cache.Add(current);
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

        public bool IsEmpty => Root == null;

        private Node Get(TKey key,out int lastComp)
        {
            Node current = Root;

            while (true)
            {
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

        public void Insert(TValue obj, TKey key, out TValue previous, bool replace)
        {
            if (Root == null)
            {
                previous = default;
                lock (Locker) Root = Cache.Get(obj);
                return;
            }

            var q = Get(key,out var c);

            previous = q.value;

            if (c == 0)
                if (replace)
                {
                    q.value = obj;
                    return;
                }
                else
                    return;
            else
            {
                Node next;
                lock (Locker) next = Cache.Get(obj);
                if (c > 0)
                    q.small = next;
                else
                    q.big = next;
            }
        }

        public bool Find(TKey key,out TValue result)
        {
            if (Root == null)
            {
                result = default;
                return false;
            }
            var q = Get(key, out var comp);
            result = q.value;
            return comp == 0;
        }

        private void Clear(Node node)
        {
            if (node.small != null)
                Clear(node.small);
            if (node.big != null)
                Clear(node.big);
            Cache.Add(node);
        }

        public System.Collections.Generic.IEnumerator<TValue> GetEnumerator()
        {
            if (Root != null)
                foreach (var q in Root.Enumerate())
                    yield return q.value;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}