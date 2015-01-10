using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
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
            : base(parent)
        {
            StopByObjectId(-10);
        }

        [DisableDump]
        protected override string TagTitle { get { return "text_item"; } }

        [DisableDump]
        internal override Size SimpleItemSize { get { return Parent.ElementType.SimpleItemSize ?? Size; } }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        IFeatureImplementation ISymbolProvider<ToNumberOfBase, IFeatureImplementation>.Feature(ToNumberOfBase tokenClass)
        {
            return Extension.MetaFeature(ToNumberOfBaseResult);
        }

        IFeatureImplementation ISymbolProvider<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
        {
            return Extension.FunctionFeature(ConcatArraysResult);
        }

        Result DumpPrintTokenResult(Category category) => VoidType.Result(category, DumpPrintCode, CodeArgs.Arg);

        CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);


        internal Result ToNumberOfBaseResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var target = left.Evaluate(context).ToString(Parent.ElementType.Size);
            var conversionBase = right.Evaluate(context).ToInt32();
            Tracer.Assert(conversionBase >= 2, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return RootContext.BitType.Result(category, result).Align;
        }

        [DisableDump]
        internal override IFeatureImplementation Feature { get { return Parent.Feature; } }

        [DisableDump]
        public override TypeBase ArrayElementType { get { return Parent.ArrayElementType; } }

        internal override int? SmartArrayLength(TypeBase elementType) { return Parent.SmartArrayLength(elementType); }

        Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType)
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

        /* 
         digits: "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
         ("  " type) !inject_declaration to_number_of_base : /\/\ digits index(^ ^^ (0))* ^ ^ + digits index(^ ^^(1))
         
         
         
         
         */
    }
}