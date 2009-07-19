using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;

#pragma warning disable 1911

namespace Reni.Type
{
    [Serializable]
    internal sealed class Void : TypeBase, IArray
    {
        protected override Size GetSize()
        {
            return Size.Zero;
        }

        internal override bool IsVoid { get { return true; } }
        internal override string DumpPrintText { get { return "void"; } }

        internal override SearchResult<IFeature> Search(Defineable defineable)
        {
            return defineable
                .SubSearch<IFeature, IArray>(this)
                .Or(() => base.Search(defineable));
        }

        internal override TypeBase CreatePair(TypeBase second)
        {
            return second;
        }

        protected override TypeBase CreateReversePair(TypeBase first)
        {
            return first;
        }

        internal override Result DumpPrint(Category category)
        {
            return CreateResult(category);
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            return false;
        }

        internal new static Result CreateResult(Category category)
        {
            return CreateVoid.CreateResult(category);
        }

        internal new static Result CreateResult(Category category, Func<CodeBase> getCode)
        {
            return CreateVoid.CreateResult(category, getCode);
        }

        internal new static Result CreateResult(Category category, Func<CodeBase> getCode, Func<Refs> getRefs)
        {
            return CreateVoid.CreateResult(category, getCode, getRefs);
        }

        internal override string DumpShort()
        {
            return "void";
        }

        TypeBase IArray.ElementType { get { return null; } }

        long IArray.Count { get { return 0; } }
    }
}