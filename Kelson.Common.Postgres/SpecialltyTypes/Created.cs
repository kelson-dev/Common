using System;

namespace Kelson.Common.Postgres
{
    public readonly struct Created : IComparable<Created>, IComparable<DateTimeOffset>
    {
        private readonly DateTimeOffset value;

        public Created(DateTimeOffset timestamp) => this.value = timestamp;

        public static bool operator ==(Created a, Created b) => a.value == b.value;
        public static bool operator !=(Created a, Created b) => a.value != b.value;
        public static bool operator ==(DateTimeOffset a, Created b) => a == b.value;
        public static bool operator !=(DateTimeOffset a, Created b) => a != b.value;
        public static bool operator ==(Created a, DateTimeOffset b) => a.value == b;
        public static bool operator !=(Created a, DateTimeOffset b) => a.value != b;

        public static implicit operator DateTimeOffset(Created Created) => Created.value;
        public static implicit operator Created(DateTimeOffset value) => new(value);

        public override string ToString() => value.ToString();

        public int CompareTo(Created other) => value.CompareTo(other);
        public int CompareTo(DateTimeOffset other) => value.CompareTo(other);

        public override bool Equals(object? obj) =>
            (obj is Created updatetime && value == updatetime.value)
         || (obj is DateTimeOffset dto && value == dto);

        public override int GetHashCode() => value.GetHashCode();

        public static bool operator <(Created left, Created right) => left.CompareTo(right) < 0;
        public static bool operator <=(Created left, Created right) => left.CompareTo(right) <= 0;
        public static bool operator >(Created left, Created right) => left.CompareTo(right) > 0;
        public static bool operator >=(Created left, Created right) => left.CompareTo(right) >= 0;

        public static bool operator <(DateTimeOffset left, Created right) => left.CompareTo(right) < 0;
        public static bool operator <=(DateTimeOffset left, Created right) => left.CompareTo(right) <= 0;
        public static bool operator >(DateTimeOffset left, Created right) => left.CompareTo(right) > 0;
        public static bool operator >=(DateTimeOffset left, Created right) => left.CompareTo(right) >= 0;

        public static bool operator <(Created left, DateTimeOffset right) => left.CompareTo(right) < 0;
        public static bool operator <=(Created left, DateTimeOffset right) => left.CompareTo(right) <= 0;
        public static bool operator >(Created left, DateTimeOffset right) => left.CompareTo(right) > 0;
        public static bool operator >=(Created left, DateTimeOffset right) => left.CompareTo(right) >= 0;
    }
}
