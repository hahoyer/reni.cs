using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class FullContext : Context, IReferenceInCode
    {
        private readonly DictionaryEx<int, ContextAtPosition> _contextAtPositionCache;

        internal FullContext(ContextBase contextBase, Container container)
            : base(contextBase, container)
        {
            _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>(position => new ContextAtPosition(Context, position));
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IReferenceInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }

        [IsDumpEnabled(false)]
        internal override IReferenceInCode ForCode { get { return this; } }

        protected override int Position { get { return StatementList.Count; } }

        internal override ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position);
        }

        [IsDumpEnabled(false)]
        private FullContext Context { get { return this; } }
        

    }
}