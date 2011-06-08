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
    [Serializable]
    internal sealed class FunctionalFeatureType : TypeBase, IFunctionalFeature
    {
        private readonly ICompileSyntax _body;
        private readonly PositionContainerContext _context;
        private static int _nextObjectId;

        internal FunctionalFeatureType(PositionContainerContext context, ICompileSyntax body)
            : base(_nextObjectId++)
        {
            _context = context;
            _body = body;
        }

        protected override Size GetSize() { return Size.Zero; }

        internal override string DumpShort() { return "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _context.ObjectId + "#)#"; }

        Result IFunctionalFeature.ContextOperatorFeatureApply(Category category)
        {
            return _context
                .ContextReferenceType
                .ArgResult(category);
        }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category)
        {
            return TypeBase
                .Void
                .Result(category, () => CodeBase.DumpPrintText(DumpPrintText));
        }

        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            var argsResult = argsType.ArgResult(category | Category.Type);
            return _context
                .CreateFunctionCall(category, Body, argsResult);
        }

        private ICompileSyntax Body { get { return _body; } }

        internal string DumpPrintText { get { return _body.DumpShort() + "/\\"; } }

        internal override IAccessType AccessType(Struct.Context context, int position)
        {
            return context
                .ContextReferenceType
                .FunctionalType(_functionalFeature);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        public Result CreateDumpPrintResult(Category category) { return Void.Result(category, () => CodeBase.DumpPrintText(_functionalFeature.DumpPrintText)); }
    }
}