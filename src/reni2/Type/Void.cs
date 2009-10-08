using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;

#pragma warning disable 1911

namespace Reni.Type
{
    [Serializable]
    internal sealed class Void : TypeBase
    {
        protected override Size GetSize()
        {
            return Size.Zero;
        }

        internal override bool IsVoid { get { return true; } }
        internal override string DumpPrintText { get { return "void"; } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        internal override bool IsValidRefTarget() { return false; }

        internal override TypeBase CreatePair(TypeBase second)
        {
            return second;
        }

        protected override TypeBase CreateReversePair(TypeBase first)
        {
            return first;
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

        internal override string DumpShort()
        {
            return "void";
        }
    }
}