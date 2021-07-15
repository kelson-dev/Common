using System;

namespace Kelson.Common.DataStructures.Text
{
    /// <summary>
    /// Builds strings of length up to 512 characters with exactly 1 heap allocation for the resulting string
    /// The builder is not range checked, so attempting to write more than 512 characters will wrap the index around and begin overwritting previous content.
    /// The Append operations return void to help avoid any possibiliy of an unintended copy of this struct.
    /// </summary>
    public unsafe ref struct StackStringBuilder
    {
        const int INCLUDED_BUFFER_SIZE = 1 << 9; // 512
        const int INCLUDED_BUFFER_MASK = INCLUDED_BUFFER_SIZE - 1;
        private fixed char coreBuffer[INCLUDED_BUFFER_SIZE];
        private int index;
        public const int Capacity = INCLUDED_BUFFER_SIZE;

        public override string ToString()
        {
            fixed (char* buffer = coreBuffer)
            {
                return new string(buffer, 0, index);
            }
        }

        public void Append(char c)
        {
            coreBuffer[index & INCLUDED_BUFFER_MASK] = c;
            index++;
        }

        public void Append(char c0, char c1)
        {
            coreBuffer[index & INCLUDED_BUFFER_MASK] = c0;
            index++;
            coreBuffer[index & INCLUDED_BUFFER_MASK] = c1;
            index++;
        }

        public void Append(char c0, char c1, char c2)
        {
            coreBuffer[index & INCLUDED_BUFFER_MASK] = c0;
            index++;
            coreBuffer[index & INCLUDED_BUFFER_MASK] = c1;
            index++;
            coreBuffer[index & INCLUDED_BUFFER_MASK] = c2;
            index++;
        }

        public void Append(Span<char> text)
        {
            for (int i = 0; i < text.Length; (i, index) = (i + 1, index + 1))
                coreBuffer[index & INCLUDED_BUFFER_MASK] = text[i];
        }

        public void Append(ReadOnlySpan<char> text)
        {
            for (int i = 0; i < text.Length; (i, index) = (i + 1, index + 1))
                coreBuffer[index & INCLUDED_BUFFER_MASK] = text[i];
        }

        public void Append(string text)
        {
            for (int i = 0; i < text.Length; (i, index) = (i + 1, index + 1))
                coreBuffer[index & INCLUDED_BUFFER_MASK] = text[i];
        }

        public void AppendLine()
        {
            coreBuffer[index & INCLUDED_BUFFER_MASK] = '\n';
            index++;
        }

        public void AppendLine(string text)
        {
            for (int i = 0; i < text.Length; (i, index) = (i + 1, index + 1))
                coreBuffer[index & INCLUDED_BUFFER_MASK] = text[i];
            coreBuffer[index & INCLUDED_BUFFER_MASK] = '\n';
            index++;
        }

        public void Append(long value)
        {
            long remainder;
            Span<char> digits = stackalloc char[32];
            int i = 0;
            do
            {
                (value, remainder) = (value / 10, value % 10);
                digits[i] = (char)('0' + remainder);
                i++;
            }
            while (value > 0 && i < 32);
            for (int j = i - 1; j >= 0; j--)
            {
                coreBuffer[index & INCLUDED_BUFFER_MASK] = digits[j];
                index++;
            }
        }

        public void Back() => index = index == 0 ? INCLUDED_BUFFER_MASK : index - 1;
    }
}
