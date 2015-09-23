using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    sealed class Function : Child, IFunctionContext
    {
        readonly int _order;

        internal Function(ContextBase parent, TypeBase argsType, TypeBase valueType = null)
            : base(parent)
        {
            _order = CodeArgs.NextOrder++;
            ArgsType = argsType;
            ValueType = valueType;
            StopByObjectIds();
        }

        [Node]
        internal TypeBase ArgsType { get; }
        [Node]
        TypeBase ValueType { get; }

        Size IContextReference.Size => Root.DefaultRefAlignParam.RefSize;
        int IContextReference.Order => _order;

        protected override string GetContextChildIdentificationDump()
            => ArgsType.ObjectId.ToString();

        internal override IFunctionContext ObtainRecentFunctionContext() => this;

        TypeBase IFunctionContext.ArgsType => ArgsType;

        Result IFunctionContext.CreateArgReferenceResult(Category category)
        {
            return ArgsType
                .ContextAccessResult(category.Typed, this, () => ArgsType.Size * -1)
                & category;
        }

        Result IFunctionContext.CreateValueReferenceResult(Category category)
        {
            if(ValueType == null)
                throw new ValueCannotBeUsedHereException();
            return ValueType.Pointer
                .ContextAccessResult
                (
                    category.Typed,
                    this,
                    () => (ArgsType.Size + Root.DefaultRefAlignParam.RefSize) * -1)
                & category;
        }
    }

    sealed class ValueCannotBeUsedHereException : Exception {}

    interface IFunctionContext : IContextReference
    {
        Result CreateArgReferenceResult(Category category);
        Result CreateValueReferenceResult(Category category);
        TypeBase ArgsType { get; }
    }
}