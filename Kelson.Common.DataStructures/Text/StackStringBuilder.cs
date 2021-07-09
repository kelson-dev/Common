namespace Kelson.Common.DataStructures.Text
{
    public unsafe ref struct StackStringBuilder
    {
        const int INCLUDED_BUFFER_SIZE = 256;
        const int INCLUDED_BUFFER_MASK = 0xFF;
        const string SMALL_BUFFER_EXCEPTION_MESSAGE = "A buffer of size 256 is included in StackStringBuilder by default. Passing an additional buffer to use is intended for larger buffers.";
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
    }
}
