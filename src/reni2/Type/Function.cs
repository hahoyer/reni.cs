using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;

namespace Reni.Type
{
    /// <summary>
    /// Function type.
    /// </summary>
    internal sealed class Function : Primitive
    {
        private readonly Context.Base _context;
        private readonly Syntax.Base _body;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="body"></param>
        internal Function(Context.Base context, Syntax.Base body)
        {
            _context = context;
            _body = body;
        }

        /// <summary>
        /// Environment where the function was found
        /// </summary>
        public Context.Base Context { get { return _context; } }

        /// <summary>
        /// Function bo
        /// </summary>
        public Syntax.Base Body { get { return _body; } }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Size.Create(0); } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# function(" + _body.DumpData() + ")"; } }

        /// <summary>
        /// Applies the function.
        /// </summary>
        /// <param name="callContext">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 29.10.2006 18:24
        internal override Result ApplyFunction(Category category, Context.Base callContext, Syntax.Base args)
        {
            Result argsResult = args
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
    }

    internal sealed class Property: Primitive
    {
        [DumpData(true)]
        private readonly Context.Base _context;
        [DumpData(true)]
        private readonly Syntax.Base _body;

        public Property(Context.Base context, Syntax.Base body)
        {
            _context = context;
            _body = body;
        }

        internal Base ResolvedType
        {
            get
            {
                return _body
                    .VisitType(_context)
                    .ApplyFunction(Category.Type, CreateVoid.CreateResult(Category.Type))
                    .Type;
            }
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Size.Create(0); } }

        internal override string DumpPrintText{ get { return "#(#context " + _context.ObjectId + "#)# property(" + _body.DumpData() + ")"; } }

        /// <summary>
        /// If type is property, execute it.
        /// </summary>
        /// <param name="rawResult">The result.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 30.07.2007 21:28 on HAHOYER-DELL by hh
        internal override Result UnProperty(Result rawResult, Context.Base context)
        {
            Tracer.Assert(!rawResult.Complete.HasCode || rawResult.Code.IsEmpty);
            Tracer.Assert(!rawResult.Complete.HasRefs || rawResult.Refs.IsNone);
            return _body
                .VisitType(context)
                .ApplyFunction(rawResult.Complete, CreateVoid.CreateResult(rawResult.Complete))
                ;
        }
    }
}