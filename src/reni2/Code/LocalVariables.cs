using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Code
{
    internal sealed class LocalVariables : CodeBase
    {
        private readonly string _holderNamePattern;
        private readonly CodeBase[] _codeBases;

        public LocalVariables(string holderNamePattern, IEnumerable<CodeBase> codeBases)
        {
            _holderNamePattern = holderNamePattern;
            _codeBases = codeBases.ToArray();
        }

        protected override Size GetSize() { return Reni.Size.Zero; }

        internal override CodeBase CreateFiber(FiberItem subsequentElement)
        {
            NotImplementedMethod(subsequentElement);
            return null;
        }

        internal override CSharpCodeSnippet CSharpCodeSnippet() { return CSharpGenerator.LocalVariables(_holderNamePattern, _codeBases); }
    }
}