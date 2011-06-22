using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Type
{
    internal sealed class FunctionalFeatureType<TObjectType, TFeature> : TypeBase
        where TObjectType: TypeBase
        where TFeature: IFunctionalFeature
    {
        private static int _nextObjectId;
        private readonly TObjectType _objectType;
        private readonly TFeature _functionalFeature;

        internal FunctionalFeatureType(TObjectType objectType, TFeature functionalFeature)
            : base(_nextObjectId++)
        {
            _objectType = objectType;
            _functionalFeature = functionalFeature;
        }

        protected override Size GetSize() { return _objectType.Size; }

        internal override string DumpShort() { return "(" + _objectType.DumpShort() + " " + _functionalFeature.DumpShort() + ")"; }
        internal override IFunctionalFeature FunctionalFeature() { return _functionalFeature; }
        internal override TypeBase ObjectType() { return _objectType; }
    }

    [Serializable]
    internal sealed class FunctionalFeatureType : TypeBase, IFunctionalFeature
    {
        private readonly ICompileSyntax _body;
        private readonly Structure _structure;
        private static int _nextObjectId;

        internal FunctionalFeatureType(Structure structure, ICompileSyntax body)
            : base(_nextObjectId++)
        {
            _structure = structure;
            _body = body;
        }

        protected override Size GetSize() { return Size.Zero; }

        internal override string DumpShort() { return "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _structure.ObjectId + "#)#"; }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category)
        {
            return TypeBase
                .Void
                .Result(category, () => CodeBase.DumpPrintText(DumpPrintText));
        }

        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            var argsResult = argsType.ArgResult(category | Category.Type);
            return _structure
                .CreateFunctionCall(category, Body, argsResult);
        }

        private ICompileSyntax Body { get { return _body; } }

        private new string DumpPrintText { get { return _body.DumpShort() + "/\\"; } }

        internal override IFunctionalFeature FunctionalFeature() { return this; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result DumpPrintResult(Category category)
        {
            return Void
                .Result(category, () => CodeBase.DumpPrintText(DumpPrintText));
        }
    }
}