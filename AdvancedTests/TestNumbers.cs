using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdvancedTypes;

namespace AdvancedTests
{
    [TestClass]
    public sealed class TestNumbers
    {
        private readonly System.Random randy = new();

        private readonly byte[] __bytes__ = new byte[8];

        public delegate T Func<T>(T left, T right);

        public delegate void Gen(out Precise c1, out Precise c2);

        public TestNumbers()
        {
            try
            {
                Precise.Initialize();
                System.Console.WriteLine("Hello world!");
            }
            catch(System.InvalidOperationException)
            {

            }
        }

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

        [TestMethod]
        public void AddPrecise() => Test((double d1, double d2) => d1 + d2, (Precise p1, Precise p2) => p1 + p2, (decimal d1, decimal d2) => d1 + d2, GenP, "+");

        [TestMethod]
        public void SubPrecise() => Test((double d1, double d2) => d1 - d2, (Precise p1, Precise p2) => p1 - p2, (decimal d1, decimal d2) => d1 - d2, GenP, "-");

        [TestMethod]
        public void MulPrecise() => Test((double d1, double d2) => d1 * d2, (Precise p1, Precise p2) => p1 * p2, (decimal d1, decimal d2) => d1 * d2, GenP, "*");

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
            Test((double d1, double d2) => d1 / d2, (Precise p1, Precise p2) => p1 / p2, (decimal d1, decimal d2) => d1 / d2, Gen, "/");
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
            Test((double d1, double d2) => d1 % d2, (Precise p1, Precise p2) => p1 % p2, (decimal d1, decimal d2) => d1 % d2, Gen, "%");
        }

        [TestMethod]
        public void SqrtPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = Precise.Abs(RandomPrecise());
                p2 = default;
            }
            Test((double d1, double d2) => System.Math.Sqrt(d1), (Precise p1, Precise p2) => Precise.Root(p1, 2), null, Gen, "[square root]");
        }

        [TestMethod]
        public void SqrPrecise() => Test((double d1, double d2) => d1 * d1, (Precise p1, Precise p2) => Precise.Square(p1), (decimal d1, decimal d2) => d1 * d1, GenP, "[square]");

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
            Test((double d1, double d2) => System.Math.Pow(d1, d2), (Precise p1, Precise p2) => Precise.Power(p1, (byte)p2.integer), null, Gen, "^");
        }

        [TestMethod]
        public void RootPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = Precise.Abs(RandomPrecise());
                p2 = randy.Next(1, 5);
            }
            Test((double d1, double d2) => System.Math.Pow(d1, 1 / d2), (Precise p1, Precise p2) => Precise.Root(p1, (byte)p2.integer), null, Gen, "^");
        }

        [TestMethod]
        public void CbrtPrecise()
        {
            void Gen(out Precise p1, out Precise p2)
            {
                p1 = Precise.Abs(RandomPrecise());//TODO not ABS
                p2 = default;
            }
            Test((double d1, double d2) => System.Math.Cbrt(d1), (Precise p1, Precise p2) => Precise.Root(p1, 3), null, Gen, "[cube root]");
        }

        private static void Test(Func<double> doubleFunc, Func<Precise> preciseFunc, Func<decimal> decimalFunc, Gen gen, string funcName)
        {
            double DoubleDiff = 0;
            Precise PreciseDiff = 0;
            decimal DecimalDiff = 0;

            var started = System.DateTime.Now;
            int K = 0;
            var needed = System.TimeSpan.FromSeconds(2);
            while (K < 100 || System.DateTime.Now - started < needed)
            {
                for (int counter = 0; counter < 1000; counter++)
                {
                    gen(out var p1, out var p2);

                    var _precise = preciseFunc(p1, p2);
                    var _double = doubleFunc((double)p1, (double)p2);
                    var _decimal = decimalFunc == null ? (((decimal)_precise) + (decimal)_double) / 2 : decimalFunc((decimal)p1, (decimal)p2);

                    DoubleDiff = System.Math.Max(System.Math.Abs((double)_decimal - (double)_precise), DoubleDiff);
                    PreciseDiff = Precise.Max(Precise.Abs((Precise)_double - (Precise)_decimal), PreciseDiff);
                    DecimalDiff = System.Math.Max(System.Math.Abs((decimal)_precise - (decimal)_double), DecimalDiff);

                    if (DoubleDiff > 0.01)
                        throw new AssertFailedException($"{p1}{funcName}{p2}\nPrecise:{_precise}\nDouble:{_double}\nDecimal{_double}\ndiff: {DoubleDiff}");
                }
                K++;
            }
            System.Console.WriteLine($"{K}k operations\nMax diff:\nDouble: {DoubleDiff}\nPrecise: {PreciseDiff}\nDecimal: {DecimalDiff}");
        }

        [TestMethod]
        public void Const()
        {
            static void Do(Precise p, double normal, string name)
            {
                var diff = Precise.Abs(p - new Precise(normal));
                System.Console.WriteLine($"{name}\n precise: {p}\n double: {normal}\n diff:\n{(double)diff}\n{diff.fractional}/\n18446744073709551616\n");
                if (diff > (Precise)0.001)
                    throw new AssertFailedException("Too large diff");
            }

            Do(Precise.E, System.Math.E, "e");
            Do(Precise.TAU, System.Math.Tau, "tau");
            Do(Precise.PI, System.Math.PI, "pi");

            Do(Precise.SquareRoot2, System.Math.Sqrt(2), "sqrt(2)");
            Do(Precise.SquareRoot3, System.Math.Sqrt(3), "sqrt(3)");

            Do(Precise.CubeRoot2, System.Math.Cbrt(2), "cbrt(2)");
            Do(Precise.CubeRoot3, System.Math.Cbrt(3), "cbrt(3)");

            Do(Precise.Square(Precise.SquareRoot2), 2, "sqrt(2)^2");
        }

        [TestMethod]
        public void RealMath()
        {
            var precise = new System.Collections.Generic.List<string>();
            Add(typeof(Precise), precise);
            var system = new System.Collections.Generic.List<string>();
            Add(typeof(System.MathF), system);
            Add(typeof(int), system);

            static void Add(System.Type type, System.Collections.Generic.List<string> to)
            {
                foreach (var member in type.GetMembers())
                {
                    var txt = member.Name;
                    if (!to.Contains(txt))
                        to.Add(txt);
                }
            }

            foreach (var m in precise)
                if (!system.Contains(m))
                    System.Console.WriteLine($"System has no [{m}]");
            foreach (var m in system)
                if (!precise.Contains(m))
                    System.Console.WriteLine($"Precise has no [{m}]");
            foreach (var m in system)
                if (precise.Contains(m))
                    System.Console.WriteLine($"They both have [{m}]");
        }
    }
}