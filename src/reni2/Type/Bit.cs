using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Bit : TypeBase
    {
        protected override Size GetSize() { return Size.Create(1); }

        internal override string DumpPrintText { get { return "bit"; } }
        internal override int SequenceCount { get { return 1; } }

        static internal Result SequenceDumpPrint(Category category, int count) { return CreateBit.CreateSequence(count).CreateArgResult(category).DumpPrintBitSequence(); }

        internal new Result DumpPrint(Category category) { return CreateArgResult(category).DumpPrintBitSequence(); }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(conversionFeature.IsUseConverter)
                return dest.HasConverterFromBit();

            return false;
        }

        static private CodeBase CreateSequenceOperation(Size size, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            return CreateBit
                .CreateSequence((objSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(feature, size);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        public override string Dump() { return GetType().FullName; }

        internal override string DumpShort() { return "bit"; }

        internal Result ApplySequenceOperation(ISequenceOfBitPrefixOperation definable, ContextBase callContext, Category category, ICompileSyntax @object)
        {
            var result = SequenceOperationResult
                (
                category,
                definable,
                callContext.Type(@object).UnrefSize
                );

            var objectResult = callContext.ConvertToSequence(category, @object, this);

            return result.UseWithArg(objectResult);
        }

        internal static Result SequenceOperationResult(Category category, ISequenceOfBitPrefixOperation feature, Size objSize) { return feature.SequenceOperationResult(category, objSize); }

        static internal Result PrefixSequenceOperationResult(Category category, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            var type = CreateNumber(objSize.ToInt());
            return type.CreateResult(category, () => CreateSequenceOperation(type.Size,feature, objSize));
        }
    }

    internal interface ISequenceOfBitPrefixOperation 
    {
        string CSharpNameOfDefaultOperation { get; }
        string DataFunctionName { get; }
        Result SequenceOperationResult(Category category, Size objSize);
    }

    internal interface ISequenceOfBitBinaryOperation
    {
        int ResultSize(int objBitCount, int argBitCount);
        bool IsCompareOperator { get; }
        string DataFunctionName { get; }
        string CSharpNameOfDefaultOperation { get; }
    }
}