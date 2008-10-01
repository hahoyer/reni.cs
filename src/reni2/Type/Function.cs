using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Function : TypeBase
    {
        private readonly ICompileSyntax _body;
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

        internal override Result ApplyFunction(Category category, ContextBase callContext, ICompileSyntax args)
        {
            var trace = ObjectId == -1 && callContext.ObjectId == 4 && (category.HasRefs || category.HasCode);
            StartMethodDumpWithBreak(trace, category,callContext,args);
            var rawResult = callContext
                .Result(category | Category.Type, args);
            DumpWithBreak(trace, "rawResult",rawResult);
            var argsResult = rawResult
                .PostProcessor.ArgsResult(Context.AlignBits);
            return ReturnMethodDumpWithBreak(trace, ApplyFunction(category, argsResult));
        }

        internal override Result ApplyFunction(Category category, Result argsResult)
        {
            return _context
                .RootContext
                .CreateFunctionCall(_context, category, Body, argsResult);
        }

        internal override string DumpShort()
        {
            return "context." + _context.ObjectId + ".function(" + _body.DumpShort() + ")";
        }
    }

    [Serializable]
    internal sealed class Property : TypeBase
    {
        [DumpData(true)] private readonly ICompileSyntax _body;
        [DumpData(true)] private readonly ContextBase _context;

        public Property(ContextBase context, ICompileSyntax body)
        {
            _context = context;
            _body = body;
        }

        protected override Size GetSize()
        {
            return Size.Create(0);
        }

        internal override string DumpPrintText
        {
            get { return "#(#context " + _context.ObjectId + "#)# property(" + _body.DumpShort() + ")"; }
        }

        [DumpData(false)]
        internal TypeBase ResolvedType
        {
            get
            {
                return _context
                    .Type(_body)
                    .ApplyFunction(Category.Type, CreateVoid.CreateResult(Category.Type))
                    .Type;
            }
        }

        internal override Result UnProperty(Result rawResult)
        {
            Tracer.Assert(!rawResult.Complete.HasCode || rawResult.Code.IsEmpty);
            Tracer.Assert(!rawResult.Complete.HasRefs || rawResult.Refs.IsNone);
            return _context
                .Type(_body)
                .ApplyFunction(rawResult.Complete, CreateVoid.CreateResult(rawResult.Complete))
                ;
        }

        internal override TypeBase UnProperty()
        {
            return _context
                .Type(_body)
                .ApplyFunction(Category.Type, CreateVoid.CreateResult(Category.Type))
                .Type
                ;
        }

        internal override string DumpShort()
        {
            return "context." + _context.ObjectId + ".property(" + _body.DumpShort() + ")";
        }
    }
}