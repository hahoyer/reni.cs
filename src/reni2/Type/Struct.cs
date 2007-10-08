using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Parser;

namespace Reni.Type
{
    public class Struct : Base
    {
        private readonly int _currentCompilePosition;
        private readonly Reni.Struct _struct;
        private readonly Context.Base _context;

        [Node]
        public int CurrentCompilePosition { get { return _currentCompilePosition; } }

        [Node]
        public Reni.Struct Struc { get { return _struct; } }

        [Node]
        public Context.Base Context { get { return _context; } }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal override Result MoveHandler(Category category)
        {
            return _struct.MoveHandler(category,_context,_currentCompilePosition);
        }

        public Struct(Context.Base context, Reni.Struct struc, int currentCompilePosition)
        {
            _context = context;
            _struct = struc;
            _currentCompilePosition = currentCompilePosition;
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return _struct.VisitSize(_context, 0, _currentCompilePosition); } }

        /// <summary>
        /// Determines whether [has converter to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <returns>
        /// 	<c>true</c> if [has converter to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        internal override bool HasConverterTo(Base dest)
        {
            return _struct.HasConverterTo(_context, dest);
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
        internal override bool IsConvertableToVirt(Base dest, ConversionFeature conversionFeature)
        {
            Void voidDest = dest as Void;
            if (voidDest != null)
                return _struct.IsConvertableToVoid(_context);
            return base.IsConvertableToVirt(dest, conversionFeature);
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
            return _struct.ConvertTo(category, _context, dest);
        }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        [DumpData(false)]
        public override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# (" + _struct.DumpPrintText(_context) + ")"; } }

        /// <summary>
        /// Searches the definable defineableToken at type
        /// </summary>
        /// <param name="defineableToken">The token.</param>
        /// <returns></returns>
        public override SearchResult SearchDefineable(DefineableToken defineableToken)
        {
            if (defineableToken.TokenClass.IsStructOperation)
                return new StructOperationResult(this, defineableToken, _currentCompilePosition);

            StructAccess structAccess = _struct.SearchDefineable(defineableToken.Name);
            if (structAccess != null)
                return new StructSearchResultFromStruct(_context, structAccess);

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
            return _struct.DestructorHandler
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
            return _struct.DumpPrintFromRef(category, _context,refAlignParam);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        /// created 09.02.2007 00:26
        internal override bool IsPending { get { return _struct.IsPendingType(_context); } }

        /// <summary>
        /// Visits the access to an element. Struct reference is assumed as "arg"
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        /// created 29.10.2006 19:17
        public Result AccessFromArg(Category category, int position)
        {
            Result result = _struct.VisitElementFromContextRef(_context, category, position);
            StructContainer structContainer = _context.CreateStructContainer(_struct);
            Code.Base argsRef = Code.Base
                .CreateArg(_context.RefAlignParam.RefSize)
                .CreateRefPlus(_context.RefAlignParam, Size);
            return result.ReplaceRelativeContextRef(structContainer, argsRef);
        }
    }

    internal class StructSearchResultFromStruct : SearchResult
    {
        private readonly Context.Base _context;
        private readonly StructAccess _structAccess;

        public StructSearchResultFromStruct(Context.Base context, StructAccess structAccess) : base(null)
        {
            _context = context;
            _structAccess = structAccess;
        }

        /// <summary>
        /// Creates the result for member function searched. Object is provided by use of "Arg" code element
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
        {
            throw new NotImplementedException();
        }
    }
}