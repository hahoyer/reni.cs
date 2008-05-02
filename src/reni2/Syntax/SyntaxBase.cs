using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Struct;

namespace Reni.Syntax
{
    /// <summary>
    /// Syntax tree is construced by objects of this type. 
    /// The center is a token, and it can have a left and a right leave (of type syntax).
    /// Furthermore it contains if it has been created by match of two tokens
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    internal abstract class SyntaxBase : ReniObject
    {
        static bool _isInDump;
        /// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns></returns>
        sealed public override string Dump()
        {
            var isInContainerDump = Container._isInDump;
            Container._isInDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = DumpShort();
            if (!isInDump)
                result += FilePosition;
            if(!isInContainerDump)
                result += "\n" + base.Dump();
            Container._isInDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        internal protected abstract string FilePosition { get; }

        private readonly DictionaryEx<Context.ContextBase, CacheItem> _cache =
            new DictionaryEx<Context.ContextBase, CacheItem>();

        [Node, DumpData(false)]
        public DictionaryEx<Context.ContextBase, CacheItem> Cache { get { return _cache; } }

        /// <summary>
        /// Visitor function, that uses a result cache.
        /// This function is called by other classes
        /// </summary>
        /// <param name="context">Environment</param>
        /// <param name="category">Category (is replendieshed here)</param>
        /// <returns></returns>
        [DebuggerHidden]
        public Result Visit(Context.ContextBase context, Category category)
        {
            var trace = ObjectId == -25 && category.HasCode && context.ObjectId == 5;
            //trace = false;
            StartMethodDumpWithBreak(trace, context, category);
            var cacheElem =
                _cache.Find
                    (
                    context,
                    () => new CacheItem(this, context)
                    );
            var result = cacheElem.Visit(category.Replendish());
            Tracer.Assert
                (
                category.Replendish().Dump() == result.Complete.Dump(),
                "Incorrect result. Expected: " + category.Replendish().Dump() + " found: " + result.Complete.Dump() +
                "\n" + Dump());
            if (trace && result.Complete.HasRefs && result.Refs.Count > 0)
                DumpMethodWithBreak("!!!!", context, category, "result", result);
            return ReturnMethodDump(trace, result);
        }

        /// <summary>
        /// Visitor function that should be overriden by derived classes
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        //[DebuggerHidden]
        internal virtual Result VirtVisit(Context.ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }


        /// <summary>
        /// Visitor for type only
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Type.TypeBase VisitType(Context.ContextBase context)
        {
            return Visit(context, Category.Type).Type;
        }

        /// <summary>
        /// Visitor for size only
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Size VisitSize(Context.ContextBase context)
        {
            return Visit(context, Category.Size).Size;
        }

        /// <summary>
        /// Top level visitor
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Result MainVisit(Context.ContextBase e)
        {
            return Visit(e, Category.Code | Category.Type);
        }

        public virtual long GetNumberConstant()
        {
            NotImplementedMethod();
            throw new NotImplementedException();
        }

        internal virtual SyntaxBase CreateDefinableSyntax(DefineableToken defineableToken, SyntaxBase right)
        {
            return CreateDefaultDefinableSyntax(defineableToken, right);
        }

        internal virtual SyntaxBase CreateListSyntax(Token token, SyntaxBase right)
        {
            if (right == null)
                return Struct.Create(this);
            return right.CreateListSyntaxReverse(this, token);
        }

        internal static SyntaxBase CreateListSyntax(DeclarationSyntax left)
        {
            return Struct.Create(left);
        }

        internal static SyntaxBase CreateListSyntax(ConverterSyntax left)
        {
            return Struct.Create(left);
        }

        internal virtual SyntaxBase CreateListSyntaxReverse(SyntaxBase left, Token token)
        {
            return Struct.Create(left, this);
        }

        internal virtual SyntaxBase CreateListSyntaxReverse(DeclarationSyntax left, Token token)
        {
            return Struct.Create(left, this);
        }

        public SyntaxBase CreateDefaultDefinableSyntax(DefineableToken defineableToken, SyntaxBase right)
        {
            return new Statement(new MemberElem(null, this), new MemberElem(defineableToken, right));
        }

        /// <summary>
        /// Creates the declaration syntax.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public virtual SyntaxBase CreateDeclarationSyntax(Token token, SyntaxBase right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal virtual string DumpShort()
        {
            NotImplementedMethod();
            return "";
        }

        /// <summary>
        /// Visits the syntax tree and evalues it.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resultType">Type of the index.</param>
        /// <returns></returns>
        /// created 20.05.2007 13:39 on HAHOYER-DELL by hh
        internal BitsConst VisitAndEvaluate(Context.ContextBase context, Type.TypeBase resultType)
        {
            Result compiledResult = Visit(context, Category.Code | Category.Type | Category.Refs);
            Result convertedResult = compiledResult.ConvertTo(resultType);
            return convertedResult.Evaluate();
        }

        /// <summary>
        /// What to when syntax element is surrounded by parenthesis?
        /// </summary>
        /// <returns></returns>
        /// created 19.07.2007 23:20 on HAHOYER-DELL by hh
        public virtual SyntaxBase SurroundedByParenthesis()
        {
            return this;
        }

        public SyntaxBase CreateConverterSyntax(Token token)
        {
            return new ConverterSyntax(this, token);
        }
    }
}