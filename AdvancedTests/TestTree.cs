using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AdvancedTests
{
    [TestClass]
    public sealed class TestTree
    {
        private sealed class Wrapper<T>
        {
            private readonly ICollection<T>[] All;

            public Wrapper(ICollection<T>[] all) => All = all;

            public int Count
            {
                get
                {
                    var n = All[0].Count;
                    for (int i = 1; i < All.Length; i++)
                        Assert.AreEqual(All[i].Count, n);
                    return n;
                }
            }

            private void Check()
            {
                IEnumerable<T> a = All[0];
                for (int i = 1; i < All.Length; i++)
                    a = System.Linq.Enumerable.Union(a, All[i]);
                Assert.AreEqual(Count, System.Linq.Enumerable.Count(a));
            }

            public void Add(T item)
            {
                foreach (var q in All)
                    q.Add(item);
                Check();
            }

            public void Clear()
            {
                foreach (var q in All)
                    q.Clear();
            }

            public bool Contains(T item)
            {
                var c = All[0].Contains(item);
                for (int i = 1; i < All.Length; i++)
                    Assert.AreEqual(All[i].Contains(item), c);
                return c;
            }

            public bool Remove(T item)
            {
                var c = All[0].Remove(item);
                for (int i = 1; i < All.Length; i++)
                    Assert.AreEqual(All[i].Remove(item), c);
                Check();
                return c;
            }
        }

        [TestMethod]
        public void MainTest()
        {
            var rand = new System.Random();

            var tree = new AdvancedTypes.BinaryTree<int>();

            var main = new Wrapper<int>(new ICollection<int>[] { tree, new SortedSet<int>() });

            int MAX = 400;

            for (int count = 1; count < MAX; count++)
            {
                main.Clear();

                for (int i = 0; i < count; i++)
                {
                    var n = rand.Next(MAX);
                    main.Add(n);
                }

                for (int i = 0; i < count; i++)
                {
                    var n = rand.Next(MAX);
                    main.Contains(n);
                }

                /*for (int i = 0; i < count; i++)
                {
                    var n = rand.Next(MAX);
                    main.Contains(n);
                }*/

                main.Clear();
            }
        }

        [TestMethod]
        public void Troll()
        {
            Assert.IsTrue(ReferenceEquals("", ""));
        }
    }
}