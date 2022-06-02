namespace AdvancedTypes
{
    /// <summary>
    /// Fixed-point number, 8 bytes for integer part and 8 to fractional
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public readonly struct Precise : System.IComparable, System.IComparable<Precise>, System.IEquatable<Precise>, System.IConvertible
    {
        #region Static

        public static readonly Precise Zero = new(0);
        public static readonly Precise One = new(1);
        public static readonly Precise Two = new(2);
        public static readonly Precise Half = new(0, ulong.MaxValue / 2 + 1);
        public static readonly Precise ZeroPositive = new(0, 1);
        public static readonly Precise ZeroNegative = new(-1, ulong.MaxValue);
        public static readonly Precise PositiveLargest = new(long.MaxValue, ulong.MaxValue);
        public static readonly Precise NegativeLargest = new(long.MinValue, 0);
        public static Precise QuarterPI { get; private set; }
        public static Precise HalfPI { get; private set; }
        public static Precise PI { get; private set; }
        public static Precise TAU { get; private set; }
        public static Precise E { get; private set; }
        public static Precise SquareRoot2 { get; private set; }
        public static Precise SquareRoot3 { get; private set; }
        public static Precise CubeRoot2 { get; private set; }
        public static Precise CubeRoot3{ get; private set; }
        const int SineApproxCount = 1024;
        private static Precise[] SineAppox = new Precise[SineApproxCount];

        public static void Initialize()
        {
            if (SineAppox == null)
                SineAppox = new Precise[SineApproxCount];
            else
                throw new System.InvalidOperationException("Already initialized");

            {
                TAU = Zero;

                var eight = new Precise(8);

                for (long i = 1; i < 1000000; i += 4)
                    TAU += (eight / i) - (eight / (i + 2));

                PI = TAU / 2;
                HalfPI = PI / 2;
                QuarterPI = HalfPI / 2;
            }
            {
                int Q = 32;
                long power = 1;
                for (int i = 0; i < Q; i++)
                    power *= 2;
                E = 1 / new Precise(power) + 1;
                for (int i = 0; i < Q; i++)
                    E *= E;
            }
            {
                var three = new Precise(3);
                SquareRoot2 = Root(Two, 2);
                SquareRoot3 = Root(three, 2);
                CubeRoot2 = Root(Two, 3);
                CubeRoot3 = Root(three, 3);
            }
            {
                var pi2 = PI * PI;
                for(int index = 0; index < SineApproxCount; index++)
                {
                    var angle2 = TAU * index / SineApproxCount;
                    angle2 *= angle2;
                    var result = One;
                    for (int n = 1; n < 100; n++)
                        result *= One - (angle2 / (pi2 * n * n));
                    SineAppox[index] = result;
                }
            }
        }

        #endregion

        public readonly long integer;
        public readonly ulong fractional;

        #region Constructions

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Precise(long number)
        {
            integer = number;
            fractional = 0;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Precise(double number)
        {
            integer = (long)System.Math.Floor(number);
            fractional = (ulong)((number - integer) * ulong.MaxValue);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Precise(decimal number)
        {
            integer = (long)System.Math.Floor(number);
            fractional = (ulong)((number - integer) * ulong.MaxValue);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Precise(long i, ulong f)
        {
            integer = i;
            fractional = f;
        }
        #endregion
        #region Operators

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator -(Precise num)
        {
            if (num.fractional == 0)
                return new(-num.integer, 0);
            else
                return new(-num.integer - 1, ulong.MaxValue - num.fractional + 1);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator ++(Precise from) => new(from.integer + 1, from.fractional);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator --(Precise from) => new(from.integer - 1, from.fractional);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator +(Precise a, Precise b)
        {
            var inted = a.integer + b.integer;
            var frac = a.fractional + b.fractional;
            if (frac < a.fractional || frac < b.fractional)
                inted++;
            return new(inted, frac);
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator -(Precise a, Precise b) => a + -b;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator *(Precise a, Precise b)
        {
            var absA = Abs(a);
            var absB = Abs(b);
            var intA = (ulong)absA.integer;
            var intB = (ulong)absB.integer;
            ulong inted = intA * intB;
            ulong frac = System.Math.BigMul(absA.fractional, absB.fractional, out ulong _);

            {
                inted += System.Math.BigMul(intA, absB.fractional, out ulong low);
                var prev = frac;
                frac += low;
                if (frac < prev)
                    inted++;
            }
            {
                inted += System.Math.BigMul(intB, absA.fractional, out ulong low);
                var prev = frac;
                frac += low;
                if (frac < prev)
                    inted++;
            }

            var result = new Precise((long)inted, frac);
            bool negative = (a.integer < 0) ^ (b.integer < 0);
            return negative ? -result : result;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator /(Precise a, Precise b)
        {
            if (a == One)
            {
                if (b.fractional == 0)
                    if (b.integer > 0)
                        return new(0, ulong.MaxValue / (ulong)b.integer);
                    else if (b.integer < 0)
                        return new(-1, ~(ulong.MaxValue / (ulong)b.integer));
                    else
                        throw new System.DivideByZeroException();
                else
                    throw new System.NotImplementedException();//TODO
            }
            if (a.integer == 0 && b.fractional == 0)
            {
                if (b.integer == 0)
                    throw new System.DivideByZeroException();
                else if (b.integer < 0)
                    throw new System.NotImplementedException();//TODO
                else
                    return new(0, a.fractional / (ulong)b.integer);
            }
            return a * (One / b);//TODO
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator %(Precise a, Precise b)
        {
            if (b <= Zero)
                throw new System.ArithmeticException($"B was not positive {b}");
            if (a == Zero)
                return a;
            while (a < Zero)//TODO plz no while loops
                a += b;
            while (a > b)
                a -= b;
            return a;
        }

        #endregion
        #region Math

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Square(Precise of)
        {
            of = Abs(of);
            var intOf = (ulong)of.integer;
            ulong inted = intOf * intOf;
            ulong frac = System.Math.BigMul(of.fractional, of.fractional, out ulong _);

            {
                inted += System.Math.BigMul(intOf, of.fractional, out ulong low) * 2;
                var prev = frac;
                frac += low;
                if (frac < prev)
                    inted++;

                prev = frac;
                frac += low;
                if (frac < prev)
                    inted++;
            }

            return new Precise((long)inted, frac);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Abs(Precise of) => of.integer < 0 ? -of : of;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static long Floor(Precise of) => of.integer;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Power(Precise of, byte level)
        {
            switch (level)
            {
                case 0:
                    if (of == Zero)
                        throw new System.ArithmeticException("Zero in zero power is not supported");
                    else
                        return One;
                case 1:
                    return of;
                case 2:
                    return Square(of);
                case 3:
                    return Square(of) * of;
                case 4:
                    return Square(Square(of));
                default:
                    var result = Square(Square(of));
                    for (ushort i = 4; i < level; i++)
                        result *= of;
                    return result;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Root(Precise from, byte level)
        {
            if (from < Zero)
                throw new System.NotImplementedException("Roots from negative numbers are not supported yet");//TODO
            if (level == 0)
                throw new System.InvalidOperationException("Zero root");
            if (level == 1 || from == One || from == Zero)
                return from;
            if ((level % 2 == 0) && from.integer < 0)
                throw new System.InvalidOperationException($"Can`t extract {level} root from negative number");

            Precise min, max;

            if (from > One)
            {
                min = One;
                max = from;
            }
            else
            {
                min = from;
                max = One;
            }

            Precise middle;
            do
            {
                middle = (max + min) * Half;
                var res = One;
                for (int i = 0; i < level; i++)
                    res *= middle;
                if (res == from)
                    return middle;
                else if (res > from)
                    max = middle;
                else
                    min = middle;
            }
            while (max - min > ZeroPositive);

            return middle;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Clamp(Precise value, Precise min, Precise max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Clamp01(Precise value)
        {
            if (value.integer < 0)
                return Zero;
            else if (value.integer > 1)
                return One;
            else
                return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Lerp(Precise t0, Precise t1, Precise time) => t0 * (One - time) + (t1 * time);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Sin(Precise radians)
        {
            throw new System.NotImplementedException();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Cos(Precise radians)
        {
            throw new System.NotImplementedException();
        }

        #endregion
        #region Comparsions

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Precise a, Precise b) => a.integer == b.integer && a.fractional == b.fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Precise a, Precise b) => a.integer != b.integer || a.fractional != b.fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Precise p)
                return this == p;
            else
                return false;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new System.ArgumentNullException(nameof(obj));
            if (obj is Precise p)
                return CompareTo(p);
            else
                throw new System.ArgumentException("object is not precise", nameof(obj));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Precise other)
        {
            if (integer == other.integer)
                return fractional.CompareTo(other.fractional);
            else
                return integer.CompareTo(other.integer);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(Precise other) => this == other;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return (int)(integer * (long)fractional);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Precise a, Precise b)
        {
            if (a.integer == b.integer)
                return a.fractional > b.fractional;
            else
                return a.integer > b.integer;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Precise a, Precise b)
        {
            if (a.integer == b.integer)
                return a.fractional < b.fractional;
            else
                return a.integer < b.integer;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Precise a, Precise b)
        {
            if (a.integer == b.integer)
                return a.fractional >= b.fractional;
            else
                return a.integer >= b.integer;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Precise a, Precise b)
        {
            if (a.integer == b.integer)
                return a.fractional <= b.fractional;
            else
                return a.integer <= b.integer;
        }

        #endregion
        #region Conversions

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator double(Precise q) => q.integer + (q.fractional / 18446744073709551616d);
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator decimal(Precise q) => q.integer + (q.fractional / 18446744073709551616m);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator Precise(decimal q) => new(q);
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator Precise(double q) => new(q);
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static implicit operator Precise(long q) => new(q, 0);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override string ToString() => ((double)this).ToString();

        #endregion
        #region Convertible

        System.TypeCode System.IConvertible.GetTypeCode() => System.TypeCode.Object;

        bool System.IConvertible.ToBoolean(System.IFormatProvider provider) => throw new System.NotSupportedException();

        byte System.IConvertible.ToByte(System.IFormatProvider provider) => (byte)integer;

        char System.IConvertible.ToChar(System.IFormatProvider provider) => (char)integer;

        System.DateTime System.IConvertible.ToDateTime(System.IFormatProvider provider) => new(integer);

        decimal System.IConvertible.ToDecimal(System.IFormatProvider provider) => (decimal)this;

        double System.IConvertible.ToDouble(System.IFormatProvider provider) => (double)this;

        short System.IConvertible.ToInt16(System.IFormatProvider provider) => (short)integer;

        int System.IConvertible.ToInt32(System.IFormatProvider provider) => (int)integer;

        long System.IConvertible.ToInt64(System.IFormatProvider provider) => integer;

        sbyte System.IConvertible.ToSByte(System.IFormatProvider provider) => (sbyte)integer;

        float System.IConvertible.ToSingle(System.IFormatProvider provider) => (float)(double)this;

        string System.IConvertible.ToString(System.IFormatProvider provider) => ToString();

        object System.IConvertible.ToType(System.Type conversionType, System.IFormatProvider provider) => throw new System.NotSupportedException();

        ushort System.IConvertible.ToUInt16(System.IFormatProvider provider) => (ushort)integer;

        uint System.IConvertible.ToUInt32(System.IFormatProvider provider) => (uint)integer;

        ulong System.IConvertible.ToUInt64(System.IFormatProvider provider) => (ulong)integer;
        #endregion
    }
}