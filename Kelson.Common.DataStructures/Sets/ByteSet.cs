using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// An immutable set of the values representable by a byte
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ByteSet : IImmutableSet<byte>, IEquatable<ByteSet>, IEnumerable<byte>
    {
        private readonly Vector<ulong> content;

        public unsafe ByteSet(ulong v0, ulong v1, ulong v2, ulong v3)
        {
            fixed (void* this_ptr = &this)
            {
                ulong* ulong_this_ptr = (ulong*)this_ptr;
                ulong_this_ptr[0] = v0;
                ulong_this_ptr[1] = v1;
                ulong_this_ptr[2] = v2;
                ulong_this_ptr[3] = v3;
            }
        }

        public ByteSet(Vector<ulong> vec) => content = vec;

        public ByteSet(params byte[] values)
        {
            var workingSet = Vector256<ulong>.Zero.AsVector();
            for (int i = 0; i < values.Length; i++)
                workingSet = Vector.BitwiseOr(workingSet, WithIndex(values[i]));
            content = workingSet;
        }

        public ByteSet(IEnumerable<byte> values)
        {
            var workingSet = Vector256<ulong>.Zero.AsVector();
            if (values is byte[] dataArray)
                for (int i = 0; i < dataArray.Length; i++)
                    workingSet = Vector.BitwiseOr(workingSet, WithIndex(dataArray[i]));
            else if (values is IList<byte> dataList)
                for (int i = 0; i < dataList.Count; i++)
                    workingSet = Vector.BitwiseOr(workingSet, WithIndex(dataList[i]));
            else
                foreach (var value in values)
                    workingSet = Vector.BitwiseOr(workingSet, WithIndex(value));
            content = workingSet;
        }

        public ByteSet(ulong each64BitLane) => content = new Vector<ulong>(each64BitLane);

        public unsafe ReadOnlySpan<ulong> AsSpan()
        {
            fixed (void* data_ptr = &content)
            {
                ulong* ulong_data_ptr = (ulong*)data_ptr;
                return new ReadOnlySpan<ulong>(ulong_data_ptr, 4);
            }
        }

        public static ByteSet All => new(~0UL);
        public static ByteSet None => new(0UL);

        /// <summary>
        /// Generates a Vector256 of ulongs with exactly one bit set, at the specified index in the range [0, 255] (any byte is valid)
        /// </summary>
        private static Vector<ulong> WithIndex(byte index)
        {
            int column = index >> 6; // the top 2 bits indicate which ulong will contain the set bit (0, 1, 2 or 3)
            int bit = index & 63; // the remaining bottom 6 bits indicate which bit to set (0 through 63)
            var data = Vector256<ulong>.Zero;
            return data.WithElement(column, 1UL << bit).AsVector();
        }

        /// <summary>
        /// Jump table mapping all byte values to the number of bits in that byte
        /// </summary>
        private static readonly byte[] BITCOUNTS = new byte[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

        /// <summary>
        /// Enumerates set to determine number of values
        /// </summary>
        public int Count
        {
            get
            {
                int sum = 0;
                sum += CountBits(content[0]);
                sum += CountBits(content[1]);
                sum += CountBits(content[2]);
                sum += CountBits(content[3]);
                return sum;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int CountBits(ulong value) =>
            BITCOUNTS[value & 0xFF]
            + BITCOUNTS[(value >> 8) & 0xFF]
            + BITCOUNTS[(value >> 16) & 0xFF]
            + BITCOUNTS[(value >> 24) & 0xFF]
            + BITCOUNTS[(value >> 32) & 0xFF]
            + BITCOUNTS[(value >> 40) & 0xFF]
            + BITCOUNTS[(value >> 48) & 0xFF]
            + BITCOUNTS[(value >> 56) & 0xFF];

        public bool IsEmpty => Vector.EqualsAll(content, Vector<ulong>.Zero);
        public bool HasAny => Vector.GreaterThanOrEqualAny(content, Vector<ulong>.Zero);
        public bool IsFull => Vector.EqualsAll(content, All.content);

        public static ByteSet operator >>(ByteSet s, int shift) => s.ShiftDown((uint)shift);

        public static ByteSet operator <<(ByteSet s, int shift) => s.ShiftUp((uint)shift);

        /// <summary>
        /// Reduces the values of each element of the set by the specified amount, losing any elements that fall out of the valid range
        /// </summary>
        public ByteSet ShiftDown(uint shift)
        {
            uint lanes = shift >> 6; // number of whole lanes shifted over (shift / 64)
            int s;
            int si;
            ulong carryMask;
            switch (lanes)
            {
                case 0:
                    s = (int)shift;
                    si = 64 - s;
                    carryMask = ((~0UL) >> (si - 1)) >> 1; // 2 step shift because ulong >> 64 is undefined behavior
                    var carried = Vector.BitwiseAnd(content, new Vector<ulong>(carryMask));
                    return new(
                        (content[0] >> s) | carried[1] << si,
                        (content[1] >> s) | carried[2] << si,
                        (content[2] >> s) | carried[3] << si,
                         content[3] >> s);
                case 1:
                    s = (int)shift & 63; // shift as int, mod 64
                    si = 64 - s;
                    carryMask = ((~0UL) >> (si - 1)) >> 1; // 2 step shift because ulong >> 64 is undefined behavior
                    return new(
                        (content[1] >> s) | (content[2] & carryMask) << si,
                        (content[2] >> s) | (content[3] & carryMask) << si,
                         content[3] >> s,
                        0UL);
                case 2:
                    s = (int)shift & 63; // shift as int, mod 64
                    si = 64 - s;
                    carryMask = ((~0UL) >> (si - 1)) >> 1; // 2 step shift because ulong >> 64 is undefined behavior
                    return new(
                        (content[2] >> s) | (content[3] & carryMask) << si,
                         content[3] >> s,
                        0UL,
                        0UL);
                case 3:
                    s = (int)shift & 63; // shift as int, mod 64
                    var last = content[3];
                    return new(
                        last >> s,
                        0UL,
                        0UL,
                        0UL);
                default:
                    return new(0UL);
            }
        }

        /// <summary>
        /// Increases the values of each element of the set by the specified amount, losing any elements that fall out of the valid range
        /// </summary>
        public ByteSet ShiftUp(uint shift)
        {
            uint lanes = shift >> 6; // number of whole lanes shifted over (shift / 64)
            int s;
            int si;
            ulong carryMask;
            switch (lanes)
            {
                case 0:
                    s = (int)shift;
                    si = 64 - s;
                    carryMask = ((~0UL) << (si - 1)) << 1; // 2 step shift because ulong >> 64 is undefined behavior
                    var carried = Vector.BitwiseAnd(content, new Vector<ulong>(carryMask));
                    return new(
                         content[0] << s,
                        (content[1] << s) | carried[0] >> si,
                        (content[2] << s) | carried[1] >> si,
                        (content[3] << s) | carried[2] >> si);
                case 1:
                    s = (int)shift & 63; // shift as int, mod 64
                    si = 64 - s;
                    carryMask = ((~0UL) << (si - 1)) << 1; // 2 step shift because ulong >> 64 is undefined behavior
                    return new(
                        (content[1] << s) | (content[2] & carryMask) >> si,
                        (content[2] << s) | (content[3] & carryMask) >> si,
                         content[3] << s,
                        0UL);
                case 2:
                    s = (int)shift & 63; // shift as int, mod 64
                    si = 64 - s;
                    carryMask = ((~0UL) << (si - 1)) << 1; // 2 step shift because ulong >> 64 is undefined behavior
                    return new(
                        (content[2] << s) | (content[3] & carryMask) >> si,
                         content[3] << s,
                        0UL,
                        0UL);
                case 3:
                    s = (int)shift & 63; // shift as int, mod 64
                    var last = content[3];
                    return new(
                        last << s,
                        0UL,
                        0UL,
                        0UL);
                default:
                    return new(0UL);
            }
        }

        /// <summary>
        /// Shifts a set as if it contained the adjacent set beyond its normal range
        /// </summary>
        public ByteSet Shift(int shift, ByteSet adjacent)
        {
            if (shift < 0)
            {
                var shifted = ShiftDown((uint)-shift);
                adjacent = adjacent.ShiftUp((uint)Math.Clamp(256 + shift, 0, 256));
                return shifted.Union(adjacent);
            }
            else
            {
                var shifted = ShiftUp((uint)shift);
                adjacent = adjacent.ShiftDown((uint)Math.Clamp(-256 + shift, 0, 256));
                return shifted.Union(adjacent);
            }
        }

        IImmutableSet<byte> IImmutableSet<byte>.Add(byte value) => new ByteSet(Vector.BitwiseOr(content, WithIndex(value)));

        public ByteSet Add(byte value) => new(Vector.BitwiseOr(content, WithIndex(value)));

        public ByteSet Add(params byte[] values) => this | new ByteSet(values);

        public ByteSet AddRange(IEnumerable<byte> values) => this | new ByteSet(values);

        IImmutableSet<byte> IImmutableSet<byte>.Clear() => new ByteSet(0UL);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteSet Clear() => new(0UL);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(byte value) => (content[value >> 6] & (0UL << (value & 63))) != 0UL; // "lane & bit is non-zero"

        IImmutableSet<byte> IImmutableSet<byte>.Except(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return Except(set);
            ByteSet asSet = new(other);
            return Except(asSet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteSet Except(ByteSet other) => new(Vector.AndNot(content, other.content));

        IImmutableSet<byte> IImmutableSet<byte>.Intersect(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return Intersect(set);
            return Intersect(new(other));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteSet Intersect(ByteSet other) => this & other;

        bool IImmutableSet<byte>.IsProperSubsetOf(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return IsProperSubsetOf(set);
            var v = this;
            bool foundOther = false;
            foreach (var item in other)
            {
                if (!Contains(item))
                    foundOther = true;
                else
                    v = v.Remove(item);
                if (v.IsEmpty && foundOther)
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSubsetOf(ByteSet other)
        {
            var dif = this ^ other;
            return (this & dif).IsEmpty && (other & dif).HasAny;
        }

        bool IImmutableSet<byte>.IsProperSupersetOf(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return IsProperSupersetOf(set);
            var v = this;
            foreach (var item in other)
            {
                if (!Contains(item))
                    return false;
                else
                    v = v.Remove(item);
            }
            return !v.IsEmpty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSupersetOf(ByteSet other)
        {
            var dif = this ^ other;
            return (this & dif).HasAny && (other & dif).IsEmpty;
        }

        bool IImmutableSet<byte>.IsSubsetOf(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return IsSubsetOf(set);
            // if all values in `this` are found in `other` return true
            var v = this;
            foreach (var item in other)
            {
                v = v.Remove(item);
                if (v.IsEmpty)
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSubsetOf(ByteSet other) => !(this & (this ^ other)).IsEmpty;

        bool IImmutableSet<byte>.IsSupersetOf(IEnumerable<byte> other)
        {
            // if all values in `other` are found in `this` return true
            foreach (var item in other)
                if (!Contains(item))
                    return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupersetOf(ByteSet other) => this == other || (this & (this ^ other)).HasAny;

        bool IImmutableSet<byte>.Overlaps(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return Overlaps(set);
            foreach (var item in other)
                if (Contains(item))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(ByteSet other) => (this & other).HasAny;

        IImmutableSet<byte> IImmutableSet<byte>.Remove(byte value) => Remove(value);

        public ByteSet Remove(byte value) => new(Vector.AndNot(content, WithIndex(value)));

        IImmutableSet<byte> IImmutableSet<byte>.SymmetricExcept(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return SymmetricExcept(set);
            return SymmetricExcept(new(other));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteSet SymmetricExcept(ByteSet other) => new(Vector.Xor(content, other.content));

        public bool TryGetValue(byte equalValue, out byte actualValue)
        {
            actualValue = equalValue;
            return Contains(equalValue);
        }

        IImmutableSet<byte> IImmutableSet<byte>.Union(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return Union(set);
            return Union(new(other));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteSet Union(ByteSet other) => new(Vector.BitwiseOr(content, other.content));

        bool IImmutableSet<byte>.SetEquals(IEnumerable<byte> other)
        {
            if (other is ByteSet set)
                return SetEquals(set);
            return this == new ByteSet(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetEquals(ByteSet other) => Vector.EqualsAll(content, other.content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ByteSet other) => Vector.EqualsAll(content, other.content);

        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            var asBytes = Vector.AsVectorByte(content);
            for (int i = 0; i < 32; i++)
            {
                byte element = (byte)(i << 3);
                byte lane = asBytes[i];
                while (lane != 0)
                {
                    if ((lane & 1) != 0)
                        yield return element;
                    lane >>= 1;
                    element++;
                }
            }
        }

        public IEnumerator GetEnumerator() => ((IEnumerable<byte>)this).GetEnumerator();

        public static ByteSet operator ^(ByteSet a, ByteSet b) => new(Vector.Xor(a.content, b.content));
        public static ByteSet operator &(ByteSet a, ByteSet b) => new(Vector.BitwiseAnd(a.content, b.content));
        public static ByteSet operator |(ByteSet a, ByteSet b) => new(Vector.BitwiseOr(a.content, b.content));
        public static ByteSet operator ~(ByteSet a) => new(Vector.Negate(a.content));

        public static bool operator ==(ByteSet a, ByteSet b) => Vector.EqualsAll(a.content, b.content);
        public static bool operator !=(ByteSet a, ByteSet b) => !Vector.EqualsAll(a.content, b.content);

        public override bool Equals(object? obj) => (obj is ByteSet other && SetEquals(other));

        public override int GetHashCode() => content.GetHashCode();

        public override string ToString() => $"[{string.Join(", ", this)}]";
    }
}
