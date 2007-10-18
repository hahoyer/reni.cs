using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Type
{
    /// <summary>
    /// Function type.
    /// </summary>
    public class Function : Primitive
    {
        private readonly Context.Base _context;
        private readonly Syntax.Base _body;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="body"></param>
        public Function(Context.Base context, Syntax.Base body)
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
        internal override string DumpPrintText { get { return "#(#context "+ _context.ObjectId +"#)# function("+_body.DumpData()+")"; } }

        /// <summary>
        /// Applies the function.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 29.10.2006 18:24
        public override Result ApplyFunction(Context.Base context, Category category, Syntax.Base args)
        {
            Result argsResult = args
                .Visit(context, category | Category.Type)
                .Align(Context.RefAlignParam.AlignBits);
            return context
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

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Size.Create(0); } }

        internal override string DumpPrintText{ get { return "#(#context " + _context.ObjectId + "#)# property(" + _body.DumpData() + ")"; } }

        /// <summary>
        /// Creates the result for DumpPrint-call. Object is provided as reference by use of "Arg" code element
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        /// created 15.05.2007 23:42 on HAHOYER-DELL by hh
        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category,refAlignParam);
            return base.DumpPrintFromRef(category, refAlignParam);
        }
    }
}