using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Syntax
{
    /// <summary>
    /// Syntax tree is construced by objects of this type. 
    /// The center is a token, and it can have a left and a right leave (of type syntax).
    /// Furthermore it contains if it has been created by match of two tokens
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    public abstract class Base : ReniObject
    {
        /// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns></returns>
        public override string Dump()
        {
            bool isInDump = Reni.Struct._isInDump;
            Reni.Struct._isInDump = false;
            string result = isInDump ? DumpShort() : base.Dump();
            Reni.Struct._isInDump = isInDump;
            return result;
        }

        private HWClassLibrary.Helper.DictionaryEx<Context.Base, CacheItem> _cache =
            new HWClassLibrary.Helper.DictionaryEx<Context.Base, CacheItem>();

        [Node, DumpData(false)]
        public HWClassLibrary.Helper.DictionaryEx<Context.Base, CacheItem> Cache { get { return _cache; } }

        /// <summary>
        /// Visitor function, that uses a result cache.
        /// This function is called by other classes
        /// </summary>
        /// <param name="context">Environment</param>
        /// <param name="category">Category (is replendieshed here)</param>
        /// <returns></returns>
        //[DebuggerHidden]
        public Result Visit(Context.Base context, Category category)
        {
            bool trace = ObjectId == -25 && category.HasCode && context.ObjectId == 5;
            //trace = false;
            StartMethodDumpWithBreak(trace, context, category);
            CacheItem cacheElem =
                _cache.Find
                    (
                    context,
                    delegate { return new CacheItem(this, context); }
                    );
            Result result = cacheElem.Visit(category.Replendish());
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
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        //[DebuggerHidden]
        public abstract Result VirtVisit(Context.Base context, Category category);

        /// <summary>
        /// Visitor for type only
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Type.Base VisitType(Context.Base context)
        {
            return Visit(context, Category.Type).Type;
        }

        /// <summary>
        /// Visitor for size only
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Size VisitSize(Context.Base context)
        {
            return Visit(context, Category.Size).Size;
        }

        /// <summary>
        /// Top level visitor
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Result MainVisit(Context.Base e)
        {
            return Visit(e, Category.Code | Category.Type);
        }

        /// <summary>
        /// Gets the number constant if possible.
        /// </summary>
        /// <returns></returns>
        /// [created 07.05.2006 18:34]
        virtual public long GetNumberConstant()
        {
            NotImplementedMethod();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits as sequence.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <returns></returns>
        /// created 13.01.2007 23:02
        public Result VisitAsSequence(Context.Base context, Category category, Type.Base elementType)
        {
            Result rawResult = Visit(context, category | Category.Type);
            Result convResult = rawResult.Type.VisitAsSequence(category, elementType);
            return convResult.UseWithArg(rawResult);
        }

        /// <summary>
        /// Creates the definable syntax.
        /// </summary>
        /// <param name="defineableToken">The defineable token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 01.04.2007 23:06 on SAPHIRE by HH
        internal virtual Base CreateDefinableSyntax(DefineableToken defineableToken, Base right)
        {
            return CreateDefaultDefinableSyntax(defineableToken, right);
        }

        /// <summary>
        /// Creates the list syntax.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 09.04.2007 19:24 on SAPHIRE by HH
        internal virtual Base CreateListSyntax(Token token, Base right)
        {
            if (right == null)
                return Struct.Create(this);
            return right.CreateListSyntaxReverse(this, token);
        }

        internal static Base CreateListSyntax(DeclarationSyntax left)
        {
            return Struct.Create(left);
        }

        internal virtual Base CreateListSyntaxReverse(Base left, Token token)
        {
            return Struct.Create(left, this);
        }

        internal virtual Base CreateListSyntaxReverse(DeclarationSyntax left, Token token)
        {
            return Struct.Create(left, this);
        }

        public Base CreateDefaultDefinableSyntax(DefineableToken defineableToken, Base right)
        {
            return new Statement(new MemberElem(null, this), new MemberElem(defineableToken, right));
        }

        /// <summary>
        /// Creates the declaration syntax.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        virtual public Base CreateDeclarationSyntax(Token token, Base right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        virtual internal string DumpShort()
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
        internal BitsConst VisitAndEvaluate(Context.Base context, Type.Base resultType)
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
        virtual public Base SurroundedByParenthesis()
        {
            return this;
        }

    }

    internal sealed class Struct : Base
    {
        [DumpData(true)]
        private Reni.Struct _data;

        internal Struct(Reni.Struct data)
        {
            _data = data;
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return _data.Dump();
        }

        /// <summary>
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        //[DebuggerHidden]
        public override Result VirtVisit(Context.Base context, Category category)
        {
            return _data.Visit(context, category);
        }

        internal override Base CreateListSyntaxReverse(Base left, Token token)
        {
            return new Struct(Reni.Struct.Create(left, _data));
        }

        internal override Base CreateListSyntaxReverse(DeclarationSyntax left, Token token)
        {
            return new Struct(Reni.Struct.Create(left, _data));
        }

        internal static Base Create(Base left)
        {
            return new Struct(Reni.Struct.Create(left));
        }

        internal static Base Create(Base left, Base right)
        {
            return new Struct(Reni.Struct.Create(left, right));
        }

        internal static Base Create(DeclarationSyntax left)
        {
            return new Struct(Reni.Struct.Create(left));
        }
        internal static Base Create(DeclarationSyntax left, Base right)
        {
            return new Struct(Reni.Struct.Create(left, right));
        }
    }
 
    sealed internal class Void : Base
    {
        private readonly Token _token;

        internal Void(Token token)
        {
            _token = token;
        }

        internal Void()
        {
        }

        /// <summary>
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        //[DebuggerHidden]
        public override Result VirtVisit(Context.Base context, Category category)
        {
            return Type.Void.CreateResult(category);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return "()";
        }
    }

}
