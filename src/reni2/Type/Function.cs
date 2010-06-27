using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
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

        private ICompileSyntax Body { get { return _body; } }

        private Result ApplyFunction(Category category, TypeBase argsType)
        {
            var argsResult = argsType.CreateArgResult(category | Category.Type);
            return _context
                .RootContext
                .CreateFunctionCall(_context, category, Body, argsResult);
        }

        public string DumpShort() { return "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _context.ObjectId + "#)#"; }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            var trace = false;
            // ObjectId == 8 && functionalResult.ObjectId == 9884 && argsResult.ObjectId == 9898 && category.HasCode;
            StartMethodDumpWithBreak(trace, category, functionalResult, argsResult);
            Tracer.Assert(argsResult.HasType);
            var result = ApplyFunction(category, argsResult.Type).UseWithArg(argsResult);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        Result IFunctionalFeature.ContextOperatorFeatureApply(Category category) { return _context.CreateThisResult(category); }
    }
}