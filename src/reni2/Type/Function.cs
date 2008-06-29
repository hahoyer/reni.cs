using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    internal sealed class Function : Primitive
    {
        private readonly ICompileSyntax _body;
        private readonly ContextBase _context;
        private static int _nextObjectId;

        internal Function(ContextBase context, ICompileSyntax body) : base(_nextObjectId++)
        {
            _context = context;
            _body = body;
        }

        internal ContextBase Context { get { return _context; } }
        internal ICompileSyntax Body { get { return _body; } }
        internal override Size Size { get { return Size.Create(0); } }
        internal override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# function(" + _body.DumpShort() + ")"; } }

        internal override Result ApplyFunction(Category category, ContextBase callContext, ICompileSyntax args)
        {
            var argsResult = callContext
                .Result(category | Category.Type, args)
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
            return "context." + _context.ObjectId + ".function(" + _body.DumpShort() + ")";
        }
    }

    internal sealed class Property : Primitive
    {
        [DumpData(true)]
        private readonly ICompileSyntax _body;
        [DumpData(true)]
        private readonly ContextBase _context;

        public Property(ContextBase context, ICompileSyntax body)
        {
            _context = context;
            _body = body;
        }

        internal override Size Size { get { return Size.Create(0); } }
        internal override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# property(" + _body.DumpShort() + ")"; } }

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

        public override string DumpShort()
        {
            return "context." + _context.ObjectId + ".property(" + _body.DumpShort() + ")";
        }
    }
}