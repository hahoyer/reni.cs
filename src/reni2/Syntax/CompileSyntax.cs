using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;

namespace Reni.Syntax
{
    abstract internal class CompileSyntax : ParsedSyntax, ICompileSyntax
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
            var trace = ObjectId == -27 && context is ContextAtPosition && category.HasCode;
            StartMethodDumpWithBreak(trace, context, category);
            if(category.HasInternal || !(category.HasCode || category.HasRefs))
                return ReturnMethodDumpWithBreak(trace, Result(context, category).Align(context.AlignBits));
            var result = Result(context, category | Category.Internal | Category.Type).Align(context.AlignBits);
            DumpWithBreak(trace, "result", result);
            return ReturnMethodDumpWithBreak(trace, result.CreateStatement(category));
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