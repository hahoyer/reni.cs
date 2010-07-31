using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Function : ReniObject, IFunctionalFeature
    {
        private readonly ICompileSyntax _body;

        private readonly Struct.Context _context;
        private static int _nextObjectId;

        internal Function(Struct.Context context, ICompileSyntax body)
            : base(_nextObjectId++)
        {
            _context = context;
            _body = body;
        }

        string IDumpShortProvider.DumpShort() { return "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _context.ObjectId + "#)#"; }

        Result IFunctionalFeature.ContextOperatorFeatureApply(Category category) { return _context.CreateContextReference(category); }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category)
        {
            return TypeBase
                .CreateVoid
                .CreateResult(category, () => CodeBase.CreateDumpPrintText(DumpPrintText));
        }

        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            var argsResult = argsType.CreateArgResult(category | Category.Type);
            return _context
                .RootContext
                .CreateFunctionCall(_context, category, Body, argsResult);
        }

        private ICompileSyntax Body { get { return _body; } }

        private string DumpPrintText { get { return "(#(#" + ObjectId + "#)#)/\\"; } }
    }
}