using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// An immutable set of the values representable by a ushort
    /// This type will either need to by sparse and heap allocate, or be an 8KB struct...
    /// </summary>
    //[Serializable]
    //[StructLayout(LayoutKind.Sequential)]
    //public readonly struct UshortSet : IImmutableSet<ushort>, IEquatable<UshortSet>, IEnumerable<ushort>, IDisposable
    //{
    //    // Indicates which 256 value wide buckets are present
    //    // For a set that contains 0 and 1200, this set will contain values 0 and 4 (0/256 and 1200/256)
    //    private readonly ByteSet represented;

    //    // Sets representing each of the 256 value wide sections of the 65536 values
    //    // Valid lengths from 0 to 256
    //    // May be longer than Count of this.represented
    //    // Read from this.content rather than directly accessing this array
    //    // The reference is held in order to return the content array to the ArrayPool
    //    private readonly ByteSet[] _contentArray;
    //    private readonly ReadOnlyMemory<ByteSet> content;

    //    public UshortSet(in ByteSet represented, WeakReference<ByteSet[]> data)
    //    {
    //        this.represented = new(represented);
    //        if (data.TryGetTarget(out ByteSet[] incomingContent))
    //        {
    //            this._contentArray = ArrayPool<ByteSet>.Shared.Rent(incomingContent.Length);
    //            Array.Copy(incomingContent, _contentArray, incomingContent.Length);
    //            this.content = _contentArray[..incomingContent.Length];
    //        }
    //        else
    //        {
    //            throw new ArgumentException("Reference to content must outlive this constructor call");
    //        }
    //    }

    //    public UshortSet From(in ByteSet buckets, ByteSet[] data) => new(in buckets, new(data));


    //    public void Dispose()
    //    {
    //        ArrayPool<ByteSet>.Shared.Return(_contentArray, clearArray: false);
    //    }
    //}
}
