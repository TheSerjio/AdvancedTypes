using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdvancedTypes;

namespace AdvancedTests
{
    [TestClass]
    public sealed class TestNumbers
    {
        private readonly System.Random randy = new();

        private readonly byte[] __bytes__ = new byte[8];

        private Precise RandomPrecise()
        {
            var mode = randy.Next(3);
            switch (mode)
            {
                case 0:
                    randy.NextBytes(__bytes__);
                    return new(randy.Next(-3, 4), System.BitConverter.ToUInt64(__bytes__));
                case 1:
                    randy.NextBytes(__bytes__);
                    var frac = System.BitConverter.ToUInt64(__bytes__);
                    randy.NextBytes(__bytes__);
                    return new(System.BitConverter.ToInt16(__bytes__), frac);
                case 2:
                    randy.NextBytes(__bytes__);
                    return new(System.BitConverter.ToInt16(__bytes__), 0);
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
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
                    p2 = Precise.Abs(RandomPrecise());
                    while (p2 < Precise.Half)
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
                p1 = Precise.Abs(RandomPrecise());
                p2 = default;
            }
            Test((double d1, double d2) => System.Math.Sqrt(d1), (Precise p1, Precise p2) => Precise.Root(p1, 2), Gen, Conv, "[square root]");
        }

        [TestMethod]
        public void SqrPrecise() => Test((double d1, double d2) => d1 * d1, (Precise p1, Precise p2) => Precise.Square(p1), GenP, Conv, "[square]");

        [TestMethod]
        public void PowPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = Precise.Zero;
                p2 = Precise.Zero;
                while (p1 == Precise.Zero && p2 == Precise.Zero)
                {
                    p1 = Precise.Abs(RandomPrecise());
                    p1 = new(p1.integer / 10000, p1.fractional);
                    p2 = (byte)randy.Next(10);
                }
            }
            Test((double d1, double d2) => System.Math.Pow(d1, d2), (Precise p1, Precise p2) => Precise.Power(p1, (byte)p2.integer), Gen, Conv, "^");
        }

        [TestMethod]
        public void RootPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = Precise.Abs(RandomPrecise());
                p2 = randy.Next(1, 5);
            }
            Test((double d1, double d2) => System.Math.Pow(d1, 1 / d2), (Precise p1, Precise p2) => Precise.Root(p1, (byte)p2.integer), Gen, Conv, "^");
        }

        [TestMethod]
        public void CbrtPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = Precise.Abs(RandomPrecise());//TODO not ABS
                p2 = default;
            }
            Test((double d1, double d2) => System.Math.Cbrt(d1), (Precise p1, Precise p2) => Precise.Root(p1,3), Gen, Conv, "[cube root]");
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
                var diff = Precise.Abs(p - new Precise(normal));
                System.Console.WriteLine($"{name}\n precise: {p}\n double: {normal}\n diff:\n{(double)diff}\n{diff.fractional}/\n18446744073709551616\n");
                if (diff > Precise.NegativePowers[8])
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