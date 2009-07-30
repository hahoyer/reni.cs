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

        private CodeBase CreateSequenceOperation(Size size, ISequenceOfBitBinaryOperation token, Size objSize, Size argsSize)
        {
            return CreateSequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        private CodeBase CreateSequenceOperation(Size size, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            return CreateSequence((objSize.ByteAlignedSize).ToInt())
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

        internal Result ApplySequenceOperation(ISequenceOfBitBinaryOperation definable, ContextBase callContext,
                                               Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var result = SequenceOperationResult
                (
                category,
                definable,
                callContext.Type(@object).UnrefSize,
                callContext.Type(args).UnrefSize
                );

            var argsResult = callContext.ConvertToSequence(category, args, this);
            var objectResult = callContext.ConvertToSequence(category, @object, this);

            return result.UseWithArg(objectResult.CreateSequence(argsResult));
        }

        internal Result ApplySequenceOperation(ISequenceOfBitOperation definable, ContextBase callContext, Category category, ICompileSyntax @object)
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

        private Result SequenceOperationResult(Category category, ISequenceOfBitBinaryOperation definable, Size objSize, Size argsSize)
        {
            var type = CreateNumber(definable.ResultSize(objSize.ToInt(), argsSize.ToInt()));
            return type.CreateResult(category, () => CreateSequenceOperation(type.Size,definable, objSize, argsSize));
        }

        private static Result SequenceOperationResult(Category category, ISequenceOfBitOperation feature, Size objSize) { return feature.SequenceOperationResult(category, objSize); }

        internal Result PrefixSequenceOperationResult(Category category, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            var type = CreateNumber(feature.ResultSize(objSize.ToInt()));
            return type.CreateResult(category, () => CreateSequenceOperation(type.Size,feature, objSize));
        }
    }

    internal interface ISequenceOfBitPrefixOperation : ISequenceOfBitOperation
    {
        int ResultSize(int objBitCount);
    }

    internal interface ISequenceOfBitBinaryOperation
    {
        int ResultSize(int objBitCount, int argBitCount);
        bool IsCompareOperator { get; }
        string DataFunctionName { get; }
        string CSharpNameOfDefaultOperation { get; }
    }
}