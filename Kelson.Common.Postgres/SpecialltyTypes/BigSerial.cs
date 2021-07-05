using System;

namespace Kelson.Common.Postgres
{
    public readonly struct BigSerial : IComparable<BigSerial>
    {
        private readonly long value;

        public BigSerial(long value) => this.value = value;
        public BigSerial(ulong value) => this.value = (long)value;

        public static bool operator ==(BigSerial a, BigSerial b) => a.value == b.value;
        public static bool operator !=(BigSerial a, BigSerial b) => a.value != b.value;

        public static implicit operator long(BigSerial id) => id.value;
        public static implicit operator BigSerial(ulong value) => new(value);
        public static implicit operator BigSerial(long value) => new(value);

        public override string ToString() => ((ulong)value).ToString();

        public int CompareTo(BigSerial other) => value.CompareTo(other);

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return value == 0;
            else if (obj is BigSerial snowflake)
                return this == snowflake;
            else if (obj is long l)
                return value == l;
            else if (obj is ulong u)
                return (ulong)value == u;
            else
                return false;
        }

        public override int GetHashCode() => value.GetHashCode();

        public static bool operator <(BigSerial left, BigSerial right) => left.CompareTo(right) < 0;
        public static bool operator <=(BigSerial left, BigSerial right) => left.CompareTo(right) <= 0;
        public static bool operator >(BigSerial left, BigSerial right) => left.CompareTo(right) > 0;
        public static bool operator >=(BigSerial left, BigSerial right) => left.CompareTo(right) >= 0;
    }
}
