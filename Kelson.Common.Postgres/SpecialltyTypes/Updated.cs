using System;

namespace Kelson.Common.Postgres
{
    public readonly struct Updated : IComparable<Updated>, IComparable<DateTimeOffset>
    {
        private readonly DateTimeOffset value;

        public Updated(DateTimeOffset timestamp) => this.value = timestamp;

        public static bool operator ==(Updated a, Updated b) => a.value == b.value;
        public static bool operator !=(Updated a, Updated b) => a.value != b.value;
        public static bool operator ==(DateTimeOffset a, Updated b) => a == b.value;
        public static bool operator !=(DateTimeOffset a, Updated b) => a != b.value;
        public static bool operator ==(Updated a, DateTimeOffset b) => a.value == b;
        public static bool operator !=(Updated a, DateTimeOffset b) => a.value != b;

        public static implicit operator DateTimeOffset(Updated updated) => updated.value;
        public static implicit operator Updated(DateTimeOffset value) => new(value);

        public override string ToString() => value.ToString();

        public int CompareTo(Updated other) => value.CompareTo(other);
        public int CompareTo(DateTimeOffset other) => value.CompareTo(other);

        public override bool Equals(object? obj) => 
            (obj is Updated updatetime && value == updatetime.value)
         || (obj is DateTimeOffset dto && value == dto);

        public override int GetHashCode() => value.GetHashCode();

        public static bool operator <(Updated left, Updated right) => left.CompareTo(right) < 0;
        public static bool operator <=(Updated left, Updated right) => left.CompareTo(right) <= 0;
        public static bool operator >(Updated left, Updated right) => left.CompareTo(right) > 0;
        public static bool operator >=(Updated left, Updated right) => left.CompareTo(right) >= 0;

        public static bool operator <(DateTimeOffset left, Updated right) => left.CompareTo(right) < 0;
        public static bool operator <=(DateTimeOffset left, Updated right) => left.CompareTo(right) <= 0;
        public static bool operator >(DateTimeOffset left, Updated right) => left.CompareTo(right) > 0;
        public static bool operator >=(DateTimeOffset left, Updated right) => left.CompareTo(right) >= 0;

        public static bool operator <(Updated left, DateTimeOffset right) => left.CompareTo(right) < 0;
        public static bool operator <=(Updated left, DateTimeOffset right) => left.CompareTo(right) <= 0;
        public static bool operator >(Updated left, DateTimeOffset right) => left.CompareTo(right) > 0;
        public static bool operator >=(Updated left, DateTimeOffset right) => left.CompareTo(right) >= 0;
    }
}
