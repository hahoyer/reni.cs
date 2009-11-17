using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Function : TypeBase, IFunctionalFeature
    {
        private readonly ICompileSyntax _body;
        
        internal override TypeBase StripFunctional()
        {
            NotImplementedMethod();
            return null;
        }

        internal override IFunctionalFeature GetFunctionalFeature()
        {
            return this;
        }

        private readonly ContextBase _context;
        private static int _nextObjectId;

        internal Function(ContextBase context, ICompileSyntax body) : base(_nextObjectId++)
        {
            _context = context;
            _body = body;
        }

        internal ContextBase Context
        {
            get { return _context; }
        }

        internal ICompileSyntax Body
        {
            get { return _body; }
        }

        protected override Size GetSize()
        {
            return Size.Create(0);
        }

        internal override string DumpPrintText
        {
            get { return "#(#context " + _context.ObjectId + "#)# function(" + _body.DumpShort() + ")"; }
        }

        internal override bool IsValidRefTarget() { return false; }

        internal override AutomaticRef CreateAutomaticRef(RefAlignParam refAlignParam)
        {
            NotImplementedMethod(refAlignParam);
            return base.CreateAutomaticRef(refAlignParam);
        }

        internal override AssignableRef CreateAssignableRef(RefAlignParam refAlignParam)
        {
            NotImplementedMethod(refAlignParam);
            return base.CreateAssignableRef(refAlignParam);
        }

        internal override Result ApplyFunction(Category category, TypeBase argsType)
        {
            var argsResult = argsType.CreateArgResult(category|Category.Type);
            return _context
                .RootContext
                .CreateFunctionCall(_context, category, Body, argsResult);
        }

        internal override string DumpShort()
        {
            return "context." + _context.ObjectId + ".function(" + _body.DumpShort() + ")";
        }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            var trace = true;// ObjectId == 8 && functionalResult.ObjectId == 9884 && argsResult.ObjectId == 9898 && category.HasCode;
            StartMethodDumpWithBreak(trace, category,functionalResult,argsResult);
            Tracer.Assert(argsResult.HasType);
            var result = ApplyFunction(category, argsResult);
            return ReturnMethodDumpWithBreak(trace,result);
        }
    }

}                                               