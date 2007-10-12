using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.Struct
{
    public class Type : Reni.Type.Base
    {
        private readonly int _currentCompilePosition;
        private readonly Container _container;
        private readonly Reni.Context.Base _context;

        [Node]
        public int CurrentCompilePosition { get { return _currentCompilePosition; } }

        [Node]
        public Container Container { get { return _container; } }

        [Node]
        public Reni.Context.Base Context { get { return _context; } }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal override Result MoveHandler(Category category)
        {
            return _container.MoveHandler(category, _context, _currentCompilePosition);
        }

        public Type(Reni.Context.Base context, Container struc, int currentCompilePosition)
        {
            _context = context;
            _container = struc;
            _currentCompilePosition = currentCompilePosition;
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return _container.VisitSize(_context, 0, _currentCompilePosition); } }

        /// <summary>
        /// Determines whether [has converter to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <returns>
        /// 	<c>true</c> if [has converter to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        internal override bool HasConverterTo(Reni.Type.Base dest)
        {
            return _container.HasConverterTo(_context, dest);
        }

        /// <summary>
        /// Determines whether [is convertable to virt] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="conversionFeature">The conversion feature.</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to virt] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 30.01.2007 22:42
        internal override bool IsConvertableToVirt(Reni.Type.Base dest, ConversionFeature conversionFeature)
        {
            Void voidDest = dest as Void;
            if (voidDest != null)
                return _container.IsConvertableToVoid(_context);
            return base.IsConvertableToVirt(dest, conversionFeature);
        }

        /// <summary>
        /// Creates the ref code for context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 01.07.07 19:16 on HAHOYER-DELL by h
        internal override Code.Base CreateRefCodeForContext(Reni.Context.Base context)
        {
            return context.CreateRefForStruct(this);
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal override Result ConvertToVirt(Category category, Reni.Type.Base dest)
        {
            return _container.ConvertTo(category, _context, dest);
        }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        [DumpData(false)]
        public override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# (" + _container.DumpPrintText(_context) + ")"; } }

        /// <summary>
        /// Searches the definable defineableToken at type
        /// </summary>
        /// <param name="defineableToken">The token.</param>
        /// <returns></returns>
        internal override SearchResult SearchDefineable(DefineableToken defineableToken)
        {
            StructContainerSearchResult structAccess = _container.SearchDefineable(defineableToken);
            if (structAccess != null)
                return structAccess.ToSearchResult(this);

            return base.SearchDefineable(defineableToken);
        }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal override Result DestructorHandler(Category category)
        {
            return _container.DestructorHandler
                (
                _context,
                category,
                CreateRef(_context.RefAlignParam).CreateArgResult(category),
                _currentCompilePosition
                );
        }

        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            return _container.DumpPrintFromRef(category, _context, refAlignParam);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        /// created 09.02.2007 00:26
        internal override bool IsPending { get { return _container.IsPendingType(_context); } }

        /// <summary>
        /// Visits the access to an element. Struct reference is assumed as "arg"
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        /// created 29.10.2006 19:17
        public Result AccessFromArg(Category category, int position)
        {
            Result result = _container.VisitElementFromContextRef(_context, category, position);
            ContainerContext containerContext = _context.CreateStructContainer(_container);
            Code.Base argsRef = Code.Base
                .CreateArg(_context.RefAlignParam.RefSize)
                .CreateRefPlus(_context.RefAlignParam, Size);
            return result.ReplaceRelativeContextRef(containerContext, argsRef);
        }
    }
}
