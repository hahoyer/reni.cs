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

        internal string HolderNamePattern { get { return _holderNamePattern; } }
        internal CodeBase[] Data { get { return _data; } }
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

        internal override CSharpCodeSnippet CSharpCodeSnippet() { return CSharpGenerator.LocalVariables(_holderNamePattern, _data); }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.LocalVariables(this); }
    }
}