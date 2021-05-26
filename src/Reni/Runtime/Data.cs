using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;

namespace Reni.Runtime
{
    [DebuggerDisplay("{Dump}")]
    public sealed class Data
    {
        public interface IView { }

        [PublicAPI]
        public sealed class View : DumpableObject, IView
        {
            readonly Data Parent;
            readonly int StartIndex;
            readonly int Bytes;

            public View(Data parent, int startIndex, int bytes)
            {
                Parent = parent;
                StartIndex = startIndex;
                Bytes = bytes;
            }

            protected override string GetNodeDump()
                => Create(Parent.Content.Pointer(StartIndex)).AddressDump +
                    ": " +
                    Parent.DumpLengthAndRange(StartIndex, Bytes);
        }

        static readonly BiasCache BiasCache = new(100);

        public static IOutStream OutStream { internal get; set; }
        readonly byte[] Content;
        int Length;

        Data(byte[] result)
        {
            Content = result;
            Length = 0;
        }

        int StartIndex
        {
            get => Content.Length - Length;
            set
            {
                Length = Content.Length - value;
                EnsureLength();
            }
        }

        string Dump => Pointer(0).AddressDump + ": " + DataDump;

        string DataDump => DataDumpPart();

        string AddressDump => BiasCache.AddressDump(this) + "=" + FlatDump;

        string FlatDump => GetBytes().Select(i => i.ToString()).Stringify(" ");

        public static Data Create(int bytes) => new(new byte[bytes]);

        [PublicAPI]
        public static Data Create(byte[] bytes) => new(bytes)
        {
            StartIndex = 0
        };

        [PublicAPI]
        public void Push(params byte[] bytes)
        {
            StartIndex -= bytes.Length;
            bytes.CopyTo(Content, StartIndex);
        }

        [PublicAPI]
        public void SizedPush(int byteCount, params byte[] bytes)
        {
            StartIndex -= byteCount;
            var i = 0;
            for(; i < byteCount && i < bytes.Length; i++)
                Content[StartIndex + i] = bytes[i];
            for(; i < byteCount; i++)
                Content[StartIndex + i] = 0;
        }

        [PublicAPI]
        public void Push(Data data) => Push(data.GetBytes(data.Length));

        [PublicAPI]
        public Data Pull(int bytes)
        {
            var result = new Data(GetBytes(bytes));
            result.StartIndex -= bytes;
            StartIndex += bytes;
            return result;
        }

        [PublicAPI]
        public void Drop(int bytes) => StartIndex += bytes;

        [PublicAPI]
        public void Drop(int bytesBefore, int bytesAfter)
        {
            var top = Pull(bytesAfter);
            StartIndex += bytesBefore - bytesAfter;
            Push(top);
        }

        [PublicAPI]
        public byte[] GetBytes(int count)
        {
            var result = new byte[count];
            for(var i = 0; i < count; i++)
                result[i] = Content[StartIndex + i];
            return result;
        }

        [PublicAPI]
        public byte[] GetBytes() => GetBytes(Length);

        void EnsureLength() => (StartIndex >= 0).Assert();

        [PublicAPI]
        public Data Pointer(int offset) => Create(Content.Pointer(StartIndex + offset));

        [PublicAPI]
        public void PointerPlus(int offset) => Content.DoRefPlus(StartIndex, offset);

        [PublicAPI]
        public Data DePointer(int bytes) => Create(Content.Dereference(StartIndex, bytes));

        [PublicAPI]
        public Data Get(int bytes, int offset) => Create(Content.Get(StartIndex + offset, bytes));

        [PublicAPI]
        public Data GetFromBack(int bytes, int offset)
            => Create(Content.Get(Content.Length + offset, bytes));

        [PublicAPI]
        public void PrintNumber() => GetBytes().PrintNumber();

        [PublicAPI]
        public void PrintText(int itemBytes)
        {
            (itemBytes == 1).Assert();
            GetBytes(Length).PrintText();
        }

        [PublicAPI]
        public static void PrintText(string data) => data.PrintText();

        [PublicAPI]
        public void Assign(int bytes)
        {
            var right = Pull(DataHandler.RefBytes);
            var left = Pull(DataHandler.RefBytes);
            left.Content.AssignFromPointers(right.Content, bytes);
        }

        [PublicAPI]
        public void ArrayGetter(int elementBytes, int indexBytes)
        {
            var offset = Pull(indexBytes).GetBytes().Times(elementBytes, DataHandler.RefBytes);
            var baseAddress = Pull(DataHandler.RefBytes);
            Push(baseAddress.GetBytes().Plus(offset, DataHandler.RefBytes));
        }

        [PublicAPI]
        public void ArraySetter(int elementBytes, int indexBytes)
        {
            var right = Pull(DataHandler.RefBytes);
            var offset = Pull(indexBytes).GetBytes().Times(elementBytes, DataHandler.RefBytes);
            var baseAddress = Pull(DataHandler.RefBytes);
            var left = baseAddress.GetBytes().Plus(offset, DataHandler.RefBytes);
            left.AssignFromPointers(right.Content, elementBytes);
        }

        [PublicAPI]
        public Data BitCast(int bits)
        {
            var x = BitsConst.Convert(Content);
            var y = x.Resize(Size.Create(bits));
            return Create(y.ToByteArray());
        }

        string DataDumpPart()
        {
            var result = "";
            result += DumpRange(0, StartIndex);
            result += " ";
            result += DumpLengthAndRange(StartIndex, Length);
            return result;
        }

        string DumpLengthAndRange(int startIndex, int bytes)
            => "[" + bytes + ": " + DumpRange(startIndex, bytes) + "]";

        string DumpRange(int startIndex, int bytes)
            => bytes <= 0? "" : DumpRangeGenerator(startIndex, bytes).Stringify(" ");

        IEnumerable<string> DumpRangeGenerator(int startIndex, int bytes)
        {
            for(var i = 0; i < bytes;)
            {
                var address = BiasCache.Dump(Content, startIndex + i);
                if(address == null)
                {
                    yield return Content[startIndex + i].ToString();
                    i++;
                }
                else
                {
                    yield return address;
                    i += DataHandler.RefBytes;
                }
            }
        }

        [PublicAPI]
        public IView GetCurrentView(int bytes) => new View(this, StartIndex, bytes);

        [PublicAPI]
        public void Equal(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsEqual);

        [PublicAPI]
        public void LessGreater(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsNotEqual);

        [PublicAPI]
        public void Less(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsLess);

        [PublicAPI]
        public void Greater(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsGreater);

        [PublicAPI]
        public void LessEqual(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsLessEqual);

        [PublicAPI]
        public void GreaterEqual(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsGreaterEqual);

        void Compare
            (int sizeBytes, int leftBytes, int rightBytes, Func<byte[], byte[], bool> operation)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            var value = (byte)(operation(left.Content, right.Content)? -1 : 0);
            for(var i = 0; i < sizeBytes; i++)
                Push(value);
        }

        [PublicAPI]
        public void MinusPrefix(int bytes)
        {
            var data = Pull(bytes);
            data.Content.MinusPrefix();
            Push(data);
        }

        [PublicAPI]
        public void Plus(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            Push(left.Content.Plus(right.Content, sizeBytes));
        }

        [PublicAPI]
        public void Minus(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            right.MinusPrefix(rightBytes);
            Push(left.Content.Plus(right.Content, sizeBytes));
        }

        [PublicAPI]
        public void Star(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            Push(left.Content.Times(right.Content, sizeBytes));
        }
    }
}