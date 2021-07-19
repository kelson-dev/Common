using Kelson.Common.Route.Args;
using Microsoft.AspNetCore.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text.Json;

namespace Kelson.Common.Route.Web.Args
{
    public class JsonArg<T> : TextArg<HttpContext, T>
    {
        private readonly JsonSerializerOptions options;

        public JsonArg(JsonSerializerOptions options) => this.options = options;

        public override string Description { get; }
        public override string Syntax { get; }

        public override IEnumerable<string> Examples() => throw new NotImplementedException();

        public override bool Matches(HttpContext context, ref ReadOnlySpan<char> text, out T result)
        {
            if (context.Request.BodyReader.TryRead(out ReadResult read))
            {
                if (read.IsCompleted)
                {
                    if (read.Buffer.IsSingleSegment)
                        result = JsonSerializer.Deserialize<T>(read.Buffer.FirstSpan, options);
                    else
                    {
                        int length = (int)read.Buffer.Length;
                        var rentedBuffer = ArrayPool<byte>.Shared.Rent(length);
                        var span = rentedBuffer[..length];
                        read.Buffer.CopyTo(span);
                        try
                        {
                            result = JsonSerializer.Deserialize<T>(read.Buffer.FirstSpan, options);
                        }
                        catch (JsonException)
                        {
                            result = default;
                            return false;
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(rentedBuffer);
                        }

                    }
                    return true;
                }
            }
            result = default;
            return false;
        }
    }
}
