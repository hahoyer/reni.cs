using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;

namespace Reni.Syntax
{
    internal class CompileSyntax : ParsedSyntax, ICompileSyntax
    {
        // Used for debug only
        [Node("Cache")]
        internal readonly DictionaryEx<ContextBase, CacheItem> ResultCache = new DictionaryEx<ContextBase, CacheItem>();

        internal CompileSyntax(Token token)
            : base(token) {}

        internal CompileSyntax(Token token, int objectId)
            : base(token, objectId) {}

        string ICompileSyntax.DumpShort()
        {
            return DumpShort();
        }

        string ICompileSyntax.FilePosition()
        {
            return FilePosition();
        }

        void ICompileSyntax.AddToCache(ContextBase context, CacheItem cacheItem)
        {
            ResultCache.Add(context,cacheItem);
        }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            var trace = ObjectId >= 0 && this is Container;
            StartMethodDump(trace, context, category);
            if(category.HasInternal || !(category.HasCode || category.HasRefs))
                return ReturnMethodDump(trace, Result(context, category));
            var result = Result(context, category | Category.Internal | Category.Type);
            DumpWithBreak(trace, "result", result);
            return ReturnMethodDumpWithBreak(trace, result.CreateStatement(category, result.Internal));
        }

        internal protected virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal protected override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            return this;
        }

        [DumpData(false)]
        internal protected override ICompileSyntax ToCompileSyntax { get { return this; } }

        internal protected override IParsedSyntax CreateSyntax(Token token, IParsedSyntax right)
        {
            return new ExpressionSyntax(this, token, ToCompiledSyntaxOrNull(right));
        }
    }
}