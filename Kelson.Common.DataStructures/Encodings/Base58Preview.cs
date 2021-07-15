using System;
using System.Numerics;

namespace Kelson.Common.DataStructures.Encodings
{
    public static class Base58Preview
    {
        // 1-9, A-H, J-N, P-Z, a-k, m-z,
        public const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private static readonly int s0 = '9' - '1' + 1;
        private static readonly int s1 = 'H' - 'A';
        private static readonly int s1c = s1 + s0 + 1;
        private static readonly int s2 = 'N' - 'J';
        private static readonly int s2c = s2 + s1c + 1;
        private static readonly int s3 = 'Z' - 'P';
        private static readonly int s3c = s2c + s3 + 1;
        private static readonly int s4 = 'k' - 'a';
        private static readonly int s4c = s3c + s4 + 1;
        private static readonly int s5 = 'z' - 'm';
        private static readonly int s5c = s4c + s5 + 1;

        public static void Encode(ReadOnlySpan<byte> data, ref Span<char> buffer)
        {
            BigInteger value = new(data, isUnsigned: true);
            int written = 0;
            int end = buffer.Length - 1;
            value = BigInteger.DivRem(value, 58, out BigInteger remainder);
            buffer[end - written++] = ALPHABET[(int)remainder];
            while (value > 0 && written < buffer.Length)
            {
                value = BigInteger.DivRem(value, 58, out remainder);
                buffer[end - written++] = ALPHABET[(int)remainder];
            }
            buffer = buffer[^written..];
        }

        public static byte[] Decode(ReadOnlySpan<char> data)
        {
            var value = BigInteger.Zero;
            for (int i = 0; i < data.Length; i++)
            {
                value *= 58;
                value += ValueOf(data[i]);
            }
            return value.ToByteArray(isUnsigned: true);
        }

        private static int ValueOf(char c)
        {
            if (c >= '1' && c <= '9')
                return c - '1';
            else if (c >= 'A' && c <= 'H')
                return (c - 'A') + s0;
            else if (c >= 'J' && c <= 'N')
                return (c - 'J') + s1c;
            else if (c >= 'P' && c <= 'Z')
                return (c - 'P') + s2c;
            else if (c >= 'a' && c <= 'k')
                return (c - 'a') + s3c;
            else if (c >= 'm' && c <= 'z')
                return (c - 'm') + s4c;
            else return -1;
        }
    }
}
