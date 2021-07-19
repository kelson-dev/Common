using System;

namespace Kelson.Common.DataStructures
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Byte array comparison optimized for 64 bit systems - compares bytes 8 at a time
        /// Using a simple for loop may be faster for small arrays, or for non 64 bit environments
        /// </summary>
        public static unsafe bool IsEquivalentTo(this byte[] data, byte[] compare)
        {
            if (data.Length != compare.Length)
                return false;
            if (data.Length == 0)
                return true;
            int longLength = (data.Length >> 3) << 3;
            fixed (byte* longData = &data[0])
            {
                fixed (byte* longCompare = &compare[0])
                {
                    for (int i = 0; i < longLength; i += 8)
                        if (*(ulong*)(longData + i) != *(ulong*)(longCompare + i))
                            return false;
                }
            }
            for (int i = longLength; i < data.Length; i++)
                if (data[i] != compare[i])
                    return false;
            return true;
        }

        public static unsafe bool IsEquivalentTo(this Span<byte> data, Span<byte> compare)
        {
            if (data.Length != compare.Length)
                return false;
            if (data.Length == 0)
                return true;
            int longLength = (data.Length >> 3) << 3;
            fixed (byte* longData = &data[0])
            {
                fixed (byte* longCompare = &compare[0])
                {
                    for (int i = 0; i < longLength; i += 8)
                        if (*(ulong*)(longData + i) != *(ulong*)(longCompare + i))
                            return false;
                }
            }
            for (int i = longLength; i < data.Length; i++)
                if (data[i] != compare[i])
                    return false;
            return true;
        }
    }
}
