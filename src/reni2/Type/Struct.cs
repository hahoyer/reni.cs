using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Parser;

namespace Reni.Type
{
    public class Struct : Base
    {
        private readonly int _currentCompilePosition;
        private readonly Reni.Struct _struc;
        private readonly Context.Base _context;

        [Node]
        public int CurrentCompilePosition { get { return _currentCompilePosition; } }

        [Node]
        public Reni.Struct Struc { get { return _struc; } }

        [Node]
        public Context.Base Context { get { return _context; } }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        public override Result MoveHandler(Category category)
        {
            return _struc.MoveHandler(category,_context,_currentCompilePosition);
        }

        public Struct(Context.Base context, Reni.Struct struc, int currentCompilePosition)
        {
            _context = context;
            _struc = struc;
            _currentCompilePosition = currentCompilePosition;
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return _struc.VisitSize(_context, 0, _currentCompilePosition); } }

        /// <summary>
        /// Determines whether [has converter to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <returns>
        /// 	<c>true</c> if [has converter to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        internal override bool HasConverterTo(Base dest)
        {
            return _struc.HasConverterTo(_context, dest);
        }

        /// <summary>
        /// Determines whether [is convertable to virt] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="useConverter">if set to <c>true</c> [use converter].</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to virt] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 30.01.2007 22:42
        internal override bool IsConvertableToVirt(Base dest, bool useConverter)
        {
            Void voidDest = dest as Void;
            if (voidDest != null)
                return _struc.IsConvertableToVoid(_context);
            return base.IsConvertableToVirt(dest, useConverter);
        }

        /// <summary>
        /// Creates the ref code for context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 01.07.07 19:16 on HAHOYER-DELL by h
        internal override Code.Base CreateRefCodeForContext(Context.Base context)
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
        internal override Result ConvertToVirt(Category category, Base dest)
        {
            return _struc.ConvertTo(category, _context, dest);
        }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        [DumpData(false)]
        public override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# (" + _struc.DumpPrintText(_context) + ")"; } }

        /// <summary>
        /// Searches the definable token at type
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public override SearchResult SearchDefineable(DefineableToken token)
        {
            StructSearchResult result = _context
                .CreateStruct(_struc, _currentCompilePosition)
                .SearchDefineable(token);
            if (result != null)
                return new FoundResult(result,this);

            return base.SearchDefineable(token);
        }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        public override Result DestructorHandler(Category category)
        {
            return _struc.DestructorHandler
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
            return _struc.DumpPrintFromRef(category, _context,refAlignParam);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        /// created 09.02.2007 00:26
        internal override bool IsPending { get { return _struc.IsPendingType(_context); } }

        /// <summary>
        /// Visits the access to an element. Struct reference is assumed as "arg"
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        /// created 29.10.2006 19:17
        public Result AccessFromArg(Category category, int position)
        {
            Result result = _struc.VisitElementFromContextRef(_context, category, position);
            StructContainer structContainer = _context.CreateStructContainer(_struc);
            Code.Base argsRef = Code.Base
                .CreateArg(_context.RefAlignParam.RefSize)
                .CreateRefPlus(_context.RefAlignParam, Size);
            return result.ReplaceRelativeContextRef(structContainer, argsRef);
        }
    }

    sealed internal class FoundResult : SearchResult
    {
        [DumpData(true)]
        private readonly StructSearchResult _structSearchResult;

        public FoundResult(StructSearchResult structSearchResult, Struct obj):base(obj)
        {
            _structSearchResult = structSearchResult;
        }

        /// <summary>
        /// Obtain result
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
        {
            return _structSearchResult.VisitApply(callContext, category, args);
        }
    }
}