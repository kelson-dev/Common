using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelson.Common.DataStructures
{
    public static class ULong
    {
        public static ulong Set(this ulong data, int index)
            => (ulong)(data | (1ul << index));

        public static ulong SetMask(this ulong data, ulong mask)
            => (ulong)(data | mask);

        public static ulong Clear(this ulong data, int index)
            => (ulong)(data & ~(1ul << index));

        public static ulong ClearMask(this ulong data, ulong mask)
            => (ulong)(data & ~mask);

        public static ulong Toggle(this ulong data, int index)
            => (ulong)(data ^ (1ul << index));

        public static ulong ToggleMask(this ulong data, ulong mask)
            => (ulong)(data ^ mask);

        public static bool IsSet(this ulong data, int index)
            => ((data >> index) & 1) == 1;

        public static bool IsMask(this ulong data, ulong mask)
            => data == mask;

        public enum Bytes
        {
            First = 0,
            Second = 8,
            Third = 16,
            Fourth = 24,
            Fifth = 32,
            Sixth = 40,
            Seventh = 48,
            Eigth = 56,
        }

        public static byte Byte(this ulong data, Bytes @byte)
            => (byte)((data & 0xFFul << (int)@byte) >> (int)@byte);

        public static ulong Pack(byte first, byte second, byte third, byte fourth, byte fifth, byte sixth, byte seventh, byte eigth)
            => (ulong)first << (int)Bytes.First
             | (ulong)second << (int)Bytes.Second
             | (ulong)third << (int)Bytes.Third
             | (ulong)fourth << (int)Bytes.Fourth
             | (ulong)fifth << (int)Bytes.Fifth
             | (ulong)sixth << (int)Bytes.Sixth
             | (ulong)seventh << (int)Bytes.Seventh
             | (ulong)eigth << (int)Bytes.Eigth;

        public static (byte, byte, byte, byte, byte, byte, byte, byte) Unpack(this ulong data)
            => (data.Byte(Bytes.First),
                data.Byte(Bytes.Second),
                data.Byte(Bytes.Third),
                data.Byte(Bytes.Fourth),
                data.Byte(Bytes.Fifth),
                data.Byte(Bytes.Sixth),
                data.Byte(Bytes.Seventh),
                data.Byte(Bytes.Eigth));
    }
}
