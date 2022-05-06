namespace AdvancedTypes
{
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
        public static readonly Precise PI, TAU, E;
        public static readonly Precise SquareRoot2, SquareRoot3, CubeRoot2, CubeRoot3;
        public static readonly System.Collections.ObjectModel.ReadOnlyCollection<Precise> Powers;
        public static readonly System.Collections.ObjectModel.ReadOnlyCollection<Precise> NegativePowers;

        static Precise()
        {
            {
                int Count = 60;
                var big = new Precise[Count];
                var small = new Precise[Count];

                big[0] = One;
                small[0] = One;

                for (int i = 1; i < Count; i++)
                {
                    big[i] = big[i - 1] * Two;
                    small[i] = small[i - 1] * Half;
                }

                Powers = System.Array.AsReadOnly(big);
                NegativePowers = System.Array.AsReadOnly(small);
            }
            {
                TAU = Zero;

                Precise eight = new(8);

                for (long i = 1; i < 1000; i += 4)
                    TAU += (eight / i) - (eight / (i + 2));

                PI = TAU / 2;
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
                SquareRoot2 = Two.Root(2);
                SquareRoot3 = three.Root(2);
                CubeRoot2 = Two.Root(3);
                CubeRoot3 = three.Root(3);
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
        #region Math

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Precise Abs() => integer < 0 ? -this : this;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public long Floor() => integer;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Precise Power(byte level)
        {
            var result = One;
            for (ushort i = 0; i < level; i++)
                result *= this;
            return result;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator -(Precise num)
        {
            if (num.fractional == 0)
                return new(-num.integer, 0);
            else
                return new(-num.integer - 1, ulong.MaxValue - num.fractional + 1);
        }

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
            var absA = a.Abs();
            var absB = b.Abs();
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
                    return new Precise(1 / (double)b);//TODO
            }
            else
                return a * (One / b);//TODO
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise operator %(Precise a, Precise b)
        {
            if (b <= Zero)
                throw new System.ArithmeticException($"B was not positive {b}");
            if (a == Zero)
                return a;
            while (a < Zero)
                a += b;
            while (a > b)
                a -= b;
            return a;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Precise Operate(System.Collections.Generic.IEnumerator<Precise> endless, Precise quality, bool dispose = true)
        {
            var previous = endless.Current;
            endless.MoveNext();
            var current = endless.Current;
            endless.MoveNext();
            ulong N = 0;
            while ((N++ < 1000) || (current - previous).Abs() > quality)
            {
                previous = current;
                endless.MoveNext();
                current = endless.Current;
            }
            if (dispose)
                endless.Dispose();
            return previous;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Precise Root(byte level)
        {
            if (this < Zero)
                throw new System.NotImplementedException("Roots from negative numbers are not supported yet");
            if (level == 0)
                throw new System.InvalidOperationException("Zero root");
            if (level == 1 || this == One || this == Zero)
                return this;
            if ((level % 2 == 0) && integer < 0)
                throw new System.InvalidOperationException($"Can`t extract {level} root from negative number");

            Precise min, max;
            if (this > One)
            {
                min = One;
                max = this;
            }
            else
            {
                min = this;
                max = One;
            }

            Precise middle;
            do
            {
                middle = (max + min) * Half;
                var res = One;
                for (int i = 0; i < level; i++)
                    res *= middle;
                if (res == this)
                    return middle;
                else if (res > this)
                    max = middle;
                else
                    min = middle;
            } 
            while (max - min > ZeroPositive);

            return middle;
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