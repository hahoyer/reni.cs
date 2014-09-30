using System;
using System.Collections.Generic;
using System.Linq;
using hw.Forms;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    sealed class Function : Child, IFunctionContext
    {
        [Node]
        internal readonly TypeBase ArgsType;
        [Node]
        readonly TypeBase _valueType;
        readonly int _order;

        internal Function(ContextBase parent, TypeBase argsType, TypeBase valueType = null)
            : base(parent)
        {
            _order = CodeArgs.NextOrder++;
            ArgsType = argsType;
            _valueType = valueType;
        }

        Size IContextReference.Size { get { return Root.DefaultRefAlignParam.RefSize; } }
        int IContextReference.Order { get { return _order; } }

        protected override string ChildDumpPrintText { get { return _valueType.DumpPrintText + "(" + ArgsType.DumpPrintText + ")"; } }

        internal override IFunctionContext ObtainRecentFunctionContext() { return this; }

        Result IFunctionContext.CreateArgReferenceResult(Category category)
        {
            return ArgsType
                .ContextAccessResult(category.Typed, this, () => ArgsType.Size * -1)
                & category;
        }

        Result IFunctionContext.CreateValueReferenceResult(Category category)
        {
            if(_valueType == null)
                throw new ValueCannotBeUsedHereException();
            return _valueType.UniquePointer
                .ContextAccessResult(category.Typed, this, () => (ArgsType.Size + Root.DefaultRefAlignParam.RefSize) * -1)
                & category;
        }
    }

    sealed class ValueCannotBeUsedHereException : Exception
    {}

    interface IFunctionContext : IContextReference
    {
        Result CreateArgReferenceResult(Category category);
        Result CreateValueReferenceResult(Category category);
    }
}