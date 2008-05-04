using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    internal sealed class Function : Primitive
    {
        private readonly SyntaxBase _body;
        private readonly ContextBase _context;

        internal Function(ContextBase context, SyntaxBase body)
        {
            _context = context;
            _body = body;
        }

        internal ContextBase Context { get { return _context; } }
        internal SyntaxBase Body { get { return _body; } }
        internal override Size Size { get { return Size.Create(0); } }
        internal override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# function(" + _body.DumpData() + ")"; } }

        internal override Result ApplyFunction(Category category, ContextBase callContext, SyntaxBase args)
        {
            var argsResult = args
                .Visit(callContext, category | Category.Type)
                .Align(Context.RefAlignParam.AlignBits);
            return ApplyFunction(category, argsResult);
        }

        internal override Result ApplyFunction(Category category, Result argsResult)
        {
            return _context
                .RootContext
                .CreateFunctionCall(_context, category, Body, argsResult);
        }

        public override string DumpShort()
        {
            return "context."+_context.ObjectId + ".function(" + _body.DumpShort() + ")";
        }
    }

    internal sealed class Property : Primitive
    {
        [DumpData(true)]
        private readonly SyntaxBase _body;
        [DumpData(true)]
        private readonly ContextBase _context;

        public Property(ContextBase context, SyntaxBase body)
        {
            _context = context;
            _body = body;
        }

        internal override Size Size { get { return Size.Create(0); } }
        internal override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# property(" + _body.DumpData() + ")"; } }

        [DumpData(false)]
        internal TypeBase ResolvedType
        {
            get
            {
                return _body
                    .VisitType(_context)
                    .ApplyFunction(Category.Type, CreateVoid.CreateResult(Category.Type))
                    .Type;
            }
        }

        internal override Result UnProperty(Result rawResult, ContextBase context)
        {
            Tracer.Assert(!rawResult.Complete.HasCode || rawResult.Code.IsEmpty);
            Tracer.Assert(!rawResult.Complete.HasRefs || rawResult.Refs.IsNone);
            return _body
                .VisitType(context)
                .ApplyFunction(rawResult.Complete, CreateVoid.CreateResult(rawResult.Complete))
                ;
        }

        public override string DumpShort()
        {
            return "context." + _context.ObjectId + ".property(" + _body.DumpShort() + ")";
        }
    }
}