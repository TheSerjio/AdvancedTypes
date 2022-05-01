namespace AdvancedTypes
{
    /*
    public readonly struct Looped : System.IComparable, System.IComparable<Looped>, System.IEquatable<Looped>
    {
        public static readonly Looped Zero = new(0);
        public static readonly Looped Max = new(1);
        public static readonly Looped Half = new(ulong.MaxValue / 2);
        public static readonly Looped Tiny = new(1);

        private const ulong _smallHalf = ulong.MaxValue / 2;

        public readonly ulong fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Looped(double number)
        {
            fractional = (ulong)(number * ulong.MaxValue);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Looped(decimal number) => fractional = (ulong)(number * ulong.MaxValue);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private Looped(ulong f) => fractional = f;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Looped Raw(ulong fractional) => new(fractional);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int Round() => fractional > _smallHalf ? 1 : 0;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int Round() => fractional > _smallHalf ? 1 : 0;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Looped Add(Looped a, Looped b, bool clamp, out Looped overflowed)
        {
            overflowed = Zero;
            if (a.fractional / 2 + b.fractional / 2 > _smallHalf)
            {
                if (clamp)
                {
                    return Max;
                }
            }
            else
                return new(a.fractional + b.fractional);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Looped Sub(Looped from, Looped what, bool clamp)
        {
            if (clamp && from.fractional <= what.fractional)
                return Zero;
            else
                return new(from.fractional - what.fractional);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Looped operator *(Looped a, Looped b) => new(System.Math.BigMul(a.fractional, b.fractional, out var _));

        //equality

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Looped a, Looped b) => a.fractional == b.fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Looped a, Looped b) => a.fractional != b.fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Looped p)
                return this == p;
            else
                return false;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new System.ArgumentNullException(nameof(obj));
            if (obj is Looped p)
                return CompareTo(p);
            else
                throw new System.ArgumentException("object is not Looped", nameof(obj));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Looped other) => fractional.CompareTo(other);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(Looped other) => this == other;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (int)fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Looped a, Looped b) => a.fractional > b.fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Looped a, Looped b) => a.fractional < b.fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Looped a, Looped b) => a.fractional >= b.fractional;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Looped a, Looped b) => a.fractional <= b.fractional;

        //casting

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator double(Looped q) => (double)q.fractional / ulong.MaxValue;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator decimal(Looped q) => (decimal)q.fractional / ulong.MaxValue;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator Precise(Looped q) => new(0, q.fractional);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator Looped(decimal q) => new(q);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator Looped(double q) => new(q);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static explicit operator Looped(Precise q) => new(q.fractional);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override string ToString() => ((double)this).ToString();
    }
    */
}