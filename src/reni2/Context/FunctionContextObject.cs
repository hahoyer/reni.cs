using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    internal sealed class FunctionContextObject : ReniObject, IReferenceInCode
    {
        private readonly TypeBase _argsType;
        private readonly ContextBase _parent;

        public FunctionContextObject(TypeBase argsType, ContextBase parent)
        {
            _argsType = argsType;
            _parent = parent;
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return Parent.RefAlignParam; } }
        internal TypeBase ArgsType { get { return _argsType; } }
        internal ContextBase Parent { get { return _parent; } }

        internal Result Result(Category category, ICompileSyntax body)
        {
            NotImplementedMethod(category, body);
            return null;
        }

        internal Result CreateArgsReferenceResult(Category category) { return ArgsType.ReferenceInCode(this, category); }

    }
}