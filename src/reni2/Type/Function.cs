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
        public override string DumpPrintText { get { return "#(#context "+ _context.ObjectId +"#)# function("+_body.DumpData()+")"; } }

        internal override string DumpPrintTextFromProperty { get { return "#(#context " + _context.ObjectId + "#)# property(" + _body.DumpData() + ")"; } }

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

    internal sealed class Property: Child
    {
        public Property(Base parent)
            : base(parent)
        {
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Parent.Size; } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        public override string DumpPrintText
        {
            get
            {
                return Parent.DumpPrintTextFromProperty;
            }
        }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal override Result DestructorHandler(Category category)
        {
            return Parent.DestructorHandler(category);
        }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal override Result MoveHandler(Category category)
        {
            return Parent.MoveHandler(category);
        }

        /// <summary>
        /// Arrays the destructor.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 04.06.2006 00:51]
        internal override Result ArrayDestructorHandler(Category category, int count)
        {
            return Parent.ArrayDestructorHandler(category, count);
        }

        /// <summary>
        /// Arrays the move handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:54]
        internal override Result ArrayMoveHandler(Category category, int count)
        {
            return Parent.ArrayMoveHandler(category, count);
        }

        /// <summary>
        /// If type is property, execute it.
        /// </summary>
        /// <param name="rawResult">The result.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 30.07.2007 21:28 on HAHOYER-DELL by hh
        /// created 30.07.2007 21:40 on HAHOYER-DELL by hh
        internal override Result UnProperty(Result rawResult, Context.Base context)
        {
            bool trace = true;
            StartMethodDump(trace, rawResult, context);
            Result result = Parent.ApplyFunction(context, rawResult.Complete, new Syntax.Void());
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}