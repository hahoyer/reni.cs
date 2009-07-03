using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Void : TypeBase, IArray
    {
        protected override Size GetSize() { return Size.Zero; }

        internal override bool IsVoid { get { return true; } }
        internal override string DumpPrintText { get { return "void"; } }

        internal override SearchResult<IFeature> Search(Defineable defineable)
        {
            var arrayResult = defineable.SearchFromArray().SubTrial(this, "try common definitions for arrays");
            var result = arrayResult.SearchResultDescriptor.Convert(arrayResult.Feature,
                                                                    this);
            if(result.IsSuccessFull)
                return result;
            return base.Search(defineable).AlternativeTrial(result);
        }

        internal override TypeBase CreatePair(TypeBase second) { return second; }

        protected override TypeBase CreateReversePair(TypeBase first) { return first; }

        internal override Result DumpPrint(Category category) { return CreateResult(category); }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature) { return false; }

        internal new static Result CreateResult(Category category) { return CreateVoid.CreateResult(category); }

        internal new static Result CreateResult(Category category, Func<CodeBase> getCode) { return CreateVoid.CreateResult(category, getCode); }

        internal new static Result CreateResult(Category category, Func<CodeBase> getCode, Func<Refs> getRefs) { return CreateVoid.CreateResult(category, getCode, getRefs); }

        internal override string DumpShort() { return "void"; }

        TypeBase IArray.ElementType { get { return null; } }

        long IArray.Count { get { return 0; } }
    }
}