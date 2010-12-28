using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal sealed class List : FiberHead
    {
        private readonly CodeBase[] _data;
        private static int _nextObjectId;

        internal CodeBase[] Data { get { return _data; } }

        internal static List Create(params CodeBase[] data) { return new List(data); }
        internal static List Create(IEnumerable<CodeBase> data) { return new List(data); }

        private List(IEnumerable<CodeBase> data): base(_nextObjectId++)
        {
            _data = data.ToArray();
            foreach (var codeBase in _data)
                Tracer.Assert(!(codeBase is List));
            Tracer.Assert(_data.Length > 1);
        }

        protected override IEnumerable<CodeBase> AsList() { return _data; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            return actual.List(this);
        }

        protected override Size MaxSizeImplementation
        {
            get
            {
                var result = Size.Zero;
                var sizeSoFar = Size.Zero;
                foreach (var codeBase in _data)
                {
                    var newResult = sizeSoFar + codeBase.MaxSize;
                    sizeSoFar += codeBase.Size;
                    result = result.Max(newResult).Max(sizeSoFar);
                }
                return result;
            }
        }

        protected override Size GetSize()
        {
            return _data
                .Aggregate(Size.Zero, (size, codeBase) => size + codeBase.Size);
        }

        protected override string CSharpString(Size top)
        {
            return CSharpGenerator.List(top, ObjectId, _data);
        }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.List(_data); }
    }
}