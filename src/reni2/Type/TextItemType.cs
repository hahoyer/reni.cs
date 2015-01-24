using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class TextItemType
        : TagChild<ArrayType>
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<ToNumberOfBase, IFeatureImplementation>
            , ISymbolProvider<ConcatArrays, IFeatureImplementation>
    {
        public TextItemType(ArrayType parent)
            : base(parent) { StopByObjectId(-10); }

        [DisableDump]
        protected override string TagTitle => "text_item";

        [DisableDump]
        internal override Size SimpleItemSize => Parent.ElementType.SimpleItemSize ?? Size;

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProvider<ToNumberOfBase, IFeatureImplementation>.Feature(ToNumberOfBase tokenClass)
            => Extension.MetaFeature(ToNumberOfBaseResult);

        IFeatureImplementation ISymbolProvider<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
            => Extension.FunctionFeature
                (
                    (category, objectReference, argsType) =>
                        ConcatArraysResult(category, objectReference, argsType, tokenClass.IsMutable),
                    this);

        protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);

        Result ToNumberOfBaseResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var target = left.Evaluate(context).ToString(Parent.ElementType.Size);
            var conversionBase = right.Evaluate(context).ToInt32();
            Tracer.Assert(conversionBase >= 2, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return RootContext.BitType.Result(category, result).Align;
        }

        [DisableDump]
        internal override IFeatureImplementation Feature => Parent.Feature;

        internal override int? SmartArrayLength(TypeBase elementType) => Parent.SmartArrayLength(elementType);

        Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType, bool isMutable)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, objectReference, argsType);
            try
            {
                var result = Parent.InternalConcatArrays(category.Typed, objectReference, argsType, isMutable);
                Dump("result", result);
                BreakExecution();

                var type = (ArrayType) result.Type;
                return ReturnMethodDump(type.TextItemType.Result(category, result));
            }
            finally
            {
                EndMethodDump();
            }
        }

        /* 
         digits: "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
         ("  " type) !inject_declaration to_number_of_base : /\/\ digits index(^ ^^ (0))* ^ ^ + digits index(^ ^^(1))
         
         
         
         
         */
    }
}