using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class FunctionalBody : TypeBase, IFunctionalFeature
    {
        private static int _nextObjectId;
        private readonly ICompileSyntax _body;
        private readonly Structure _structure;

        internal FunctionalBody(Structure structure, ICompileSyntax body)
            : base(_nextObjectId++)
        {
            _structure = structure;
            _body = body;
            StopByObjectId(1);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

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

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpShort() { return base.DumpShort() + "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _structure.ObjectId + "#)#"; }
        internal override string DumpPrintText { get { return _body.DumpShort() + "/\\"; } }
        internal override IFunctionalFeature FunctionalFeature() { return this; }
        internal override TypeBase ObjectType() { return _structure.ReferenceType; }

        private ICompileSyntax Body { get { return _body; } }

        internal Result DumpPrintResult(Category category)
        {
            return TypeBase.Void
                .Result(category, () => CodeBase.DumpPrintText(DumpPrintText));
        }
    }
}