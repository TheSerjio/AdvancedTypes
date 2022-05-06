using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdvancedTypes;

namespace AdvancedTests
{
    [TestClass]
    public sealed class Test_1_Trees
    {
        [TestMethod]
        public void MainTest()
        {
            var rand = new System.Random();

            var list = new System.Collections.Generic.HashSet<int>();
            var tree = new BinaryTree<int, int>();

            for (int count = 1; count < 5000; count++)
            {
                list.Clear();

                for (int i = 0; i < count; i++)
                {
                    var n = rand.Next();
                    tree.Insert(n, n, out var _, true);
                    list.Add(n);
                }

                int some;
                using (var Enum = list.GetEnumerator())
                {
                    Enum.MoveNext();
                    some = Enum.Current;
                }

                Assert.IsTrue(tree.Find(some, out var result));
                Assert.IsTrue(result == some);
                while (!tree.IsEmpty) tree.TakeOne();
            }
        }
    }

    [TestClass]
    public sealed class Test_2_Numbers
    {
        private readonly System.Random randy = new();

        private readonly byte[] __bytes__ = new byte[8];

        private Precise RandomPrecise()
        {
            randy.NextBytes(__bytes__);
            return new(randy.Next(-10, 11), System.BitConverter.ToUInt64(__bytes__));
        }

        private void GenP(out Precise p1, out Precise p2)
        {
            p1 = RandomPrecise();
            p2 = RandomPrecise();
        }

        private double Conv(Precise p) => (double)p;

        [TestMethod]
        public void AddPrecise() => Test((double d1, double d2) => d1 + d2, (Precise p1, Precise p2) => p1 + p2, GenP, Conv, "+");

        [TestMethod]
        public void SubPrecise() => Test((double d1, double d2) => d1 - d2, (Precise p1, Precise p2) => p1 - p2, GenP, Conv, "-");

        [TestMethod]
        public void MulPrecise() => Test((double d1, double d2) => d1 * d2, (Precise p1, Precise p2) => p1 * p2, GenP, Conv, "*");

        [TestMethod]
        public void DivPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = RandomPrecise();
                lock (randy)
                {
                    p2 = RandomPrecise();
                    while (p2.Abs().fractional < 1000)
                        p2 = RandomPrecise();
                }
            }
            Test((double d1, double d2) => d1 / d2, (Precise p1, Precise p2) => p1 / p2, Gen, Conv, "/");
        }

        [TestMethod]
        public void RemPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = new Precise(randy.Next(64) * randy.NextDouble());
                p2 = Precise.Zero;
                while (p2 == Precise.Zero)
                    p2 = new Precise(randy.Next(64) * randy.NextDouble());
            }
            Test((double d1, double d2) => d1 % d2, (Precise p1, Precise p2) => p1 % p2, Gen, Conv, "%");
        }

        [TestMethod]
        public void SqrtPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = RandomPrecise().Abs();
                p2 = default;
            }
            Test((double d1, double d2) => System.Math.Sqrt(d1), (Precise p1, Precise p2) => p1.Root(2), Gen, Conv, "[square root]");
        }

        [TestMethod]
        public void CbrtPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = RandomPrecise().Abs();
                p2 = default;
            }
            Test((double d1, double d2) => System.Math.Cbrt(d1), (Precise p1, Precise p2) => p1.Root(3), Gen, Conv, "[cube root]");
        }

        delegate void Two<T>(out T t1, out T t2);

        private static void Test<T>(System.Func<double, double, double> funcD, System.Func<T, T, T> func, Two<T> gen, System.Func<T, double> ToD, string funcName)
        {
            double diff = 0;
            var started = System.DateTime.Now;
            int K = 0;
            var needed = System.TimeSpan.FromSeconds(2);
            while (K < 100 || System.DateTime.Now - started < needed)
            {
                for (int counter = 0; counter < 1000; counter++)
                {
                    gen(out var t1, out var t2);

                    var weird = ToD(func(t1, t2));

                    var correct = funcD(ToD(t1), ToD(t2));

                    diff = System.Math.Max(System.Math.Abs(weird - correct), diff);

                    if (diff > 0.01)
                        throw new AssertFailedException($"{t1}{funcName}{t2}\nWeird:{weird}\nCorrect:{correct}\ndiff: {diff}");
                }
                K++;
            }
            System.Console.WriteLine($"{K}k operations\nMax diff: {diff}");
        }

        [TestMethod]
        public void Const()
        {
            static void Do(Precise p, double normal, string name)
            {
                var diff = (p - new Precise(normal)).Abs();
                System.Console.WriteLine($"{name}\n precise: {p}\n double: {normal}\n diff:\n{(double)diff}\n{diff.fractional}/\n18446744073709551616\n");
                if (diff > Precise.NegativePowers[2])
                    throw new AssertFailedException("Too large diff");
            }

            Do(Precise.E, System.Math.E, "e");
            Do(Precise.TAU, System.Math.Tau, "tau");
            Do(Precise.PI, System.Math.PI, "pi");

            Do(Precise.SquareRoot2, System.Math.Sqrt(2), "sqrt(2)");
            Do(Precise.SquareRoot3, System.Math.Sqrt(3), "sqrt(3)");

            Do(Precise.CubeRoot2, System.Math.Cbrt(2), "cbrt(2)");
            Do(Precise.CubeRoot3, System.Math.Cbrt(3), "cbrt(3)");
        }
    }
}