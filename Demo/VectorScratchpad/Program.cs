using System;
using static System.Console;

const string testText = "||abc||a||||yge87||";

int[] expected = new int[]
{
// |  |  a  b  c  |  |  a  |  |  |  |  y  g  e  8  7  |  |  
// 0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20
   0, 1, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4
};

for (int i = 0; i < 20; i++)
{
    Write(i < testText.Length ? testText[i] : '\0');
    Write(' ');
    Write(i);
    Write(": ");
    Write(CountSpoilerBracketsUntilIndex("||abc||a||||yge87||", i));
    Write(", ");
    WriteLine(expected[i]);
}
WriteLine("Done");



int CountSpoilerBracketsUntilIndex(string text, int index)
{
    int count = 0;
    var span = text[..(Math.Min(index+1, text.Length))];
    while (span.Length > 1)
    {
        int nextBracket = span.IndexOf("||");
        if (nextBracket < 0)
            return count;
        else
        {
            span = span[(Math.Min(nextBracket + 3, span.Length))..];
            count++;
        }
    }
    return count;
}