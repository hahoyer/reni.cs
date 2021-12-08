using System;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    sealed class Function : Child, IFunctionContext
    {
        readonly int Order;

        internal Function(ContextBase parent, TypeBase argsType, TypeBase valueType = null)
            : base(parent)
        {
            Order = Closures.NextOrder++;
            ArgsType = argsType;
            ValueType = valueType;
            StopByObjectIds();
        }

        [Node]
        internal TypeBase ArgsType { get; }
        [Node]
        TypeBase ValueType { get; }

        int IContextReference.Order => Order;

        protected override string GetContextChildIdentificationDump()
            => "@(." + ArgsType.ObjectId + "i)";

        internal override IFunctionContext ObtainRecentFunctionContext() => this;
        [DisableDump]
        protected override string LevelFormat => "function";

        TypeBase IFunctionContext.ArgsType => ArgsType;

        Result IFunctionContext.CreateArgReferenceResult(Category category)
        {
            return ArgsType
                .ContextAccessResult(category.WithType, this, () => ArgsType.Size * -1)
                & category;
        }

        Result IFunctionContext.CreateValueReferenceResult(Category category)
        {
            if(ValueType == null)
                throw new ValueCannotBeUsedHereException();
            return ValueType.Pointer
                .ContextAccessResult
                (
                    category.WithType,
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