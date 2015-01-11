using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.ReniSyntax;

namespace Reni.Type
{
    sealed class TextItemsType
        : TagChild<ArrayType>
    {
        public TextItemsType(ArrayType parent)
            : base(parent) { }

        internal Result ToNumberOfBaseResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var target = left.Evaluate(context).ToString(Parent.ElementType.Size);
            var conversionBase = right.Evaluate(context).ToInt32();
            Tracer.Assert(conversionBase >= 2, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return RootContext.BitType.Result(category, result).Align;
        }

        [DisableDump]
        protected override string TagTitle { get { return "text_items"; } }

        [DisableDump]
        internal override IFeatureImplementation Feature { get { return Parent.Feature; } }

        [DisableDump]
        public override TypeBase ArrayElementType { get { return Parent.ArrayElementType; } }

        internal override int? SmartArrayLength(TypeBase elementType) { return Parent.SmartArrayLength(elementType); }

        internal Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, objectReference, argsType);
            try
            {
                var result = Parent.InternalConcatArrays(category.Typed, objectReference, argsType);
                Dump("result", result);
                BreakExecution();

                var type = (ArrayType) result.Type;
                return ReturnMethodDump(type.UniqueTextItemType.Result(category, result));
            }
            finally
            {
                EndMethodDump();
            }
        }

        protected override CodeBase DumpPrintCode()
            => UniquePointer
                .ArgCode
                .DePointer(Size)
                .DumpPrintText(Parent.ElementType.Size);
    }
}