using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    internal sealed class LocalVariables : CodeBase
    {
        private readonly string _holderNamePattern;
        private readonly CodeBase[] _data;

        internal LocalVariables(string holderNamePattern, IEnumerable<CodeBase> data)
        {
            _holderNamePattern = holderNamePattern;
            _data = data.ToArray();
            Tracer.Assert(_data.Length > 0);
        }

        [IsDumpEnabled(false)]
        internal string HolderNamePattern { get { return _holderNamePattern; } }
        [IsDumpEnabled(false)]
        internal CodeBase[] Data { get { return _data; } }

        protected override Size MaxSizeImplementation
        {
            get
            {
                return _data
                    .Select(x => x.MaxSize)
                    .Max();
            }
        }

        protected override Size GetSize() { return Size.Zero; }

        internal override CodeBase CreateFiber(FiberItem subsequentElement)
        {
            NotImplementedMethod(subsequentElement);
            return null;
        }

        [IsDumpEnabled(false)]
        public override string NodeDump
        {
            get
            {
                var result = base.NodeDump + " Holder=" + _holderNamePattern;
                var codeBases = _data.DumpLines().Surround("{","}");
                return result + " CodeBases=" + codeBases;
            }
        }

        protected override string CSharpString(Size top) { return CSharpGenerator.LocalVariables(top, ObjectId, _holderNamePattern, _data); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.LocalVariables(_holderNamePattern, _data); }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.LocalVariables(this); }
    }
}