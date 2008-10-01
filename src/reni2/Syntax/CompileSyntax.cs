using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    [Serializable]
    internal abstract class CompileSyntax : ParsedSyntax, ICompileSyntax
    {
        // Used for debug only
        [DumpData(false), Node("Cache")] internal readonly DictionaryEx<ContextBase, IResultProvider> ResultCache =
            new DictionaryEx<ContextBase, IResultProvider>();

        internal CompileSyntax(Token token)
            : base(token)
        {
        }

        internal CompileSyntax(Token token, int objectId)
            : base(token, objectId)
        {
        }

        string ICompileSyntax.DumpShort()
        {
            return DumpShort();
        }

        string ICompileSyntax.FilePosition()
        {
            return FilePosition();
        }

        void ICompileSyntax.AddToCache(ContextBase context, IResultProvider cacheItem)
        {
            ResultCache.Add(context, cacheItem);
        }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            var trace = ObjectId == -82 && context is Function && category.HasRefs;
            StartMethodDumpWithBreak(trace, context, category);
            if (category.HasInternal || !(category.HasCode || category.HasRefs))
                return ReturnMethodDumpWithBreak(trace, Result(context, category).Align(context.AlignBits));
            var result = Result(context, category | Category.Internal | Category.Type).Align(context.AlignBits);
            DumpWithBreak(trace, "result", result);
            return ReturnMethodDumpWithBreak(trace, result.CreateStatement(category));
        }

        protected internal virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        protected internal override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            return this;
        }

        [DumpData(false)]
        protected internal override ICompileSyntax ToCompileSyntax
        {
            get { return this; }
        }

        protected internal override IParsedSyntax CreateSyntax(Token token, IParsedSyntax right)
        {
            return new ExpressionSyntax(this, token, ToCompiledSyntaxOrNull(right));
        }
    }
}