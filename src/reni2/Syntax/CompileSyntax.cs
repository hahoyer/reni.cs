using HWClassLibrary.TreeStructure;
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
        [DumpData(false), Node("Cache")]
        internal readonly DictionaryEx<ContextBase, object> ResultCache = new DictionaryEx<ContextBase, object>();

        internal CompileSyntax(Token token)
            : base(token) { }

        internal CompileSyntax(Token token, int objectId)
            : base(token, objectId) { }

        string ICompileSyntax.DumpShort() { return DumpShort(); }

        string ICompileSyntax.FilePosition() { return FilePosition(); }

        void ICompileSyntax.AddToCacheForDebug(ContextBase context, object cacheItem) { ResultCache.Add(context, cacheItem); }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            return Result(context, category).Align(context.AlignBits);
        }

        internal protected virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal protected override IParsedSyntax SurroundedByParenthesis(Token token) { return this; }

        [DumpData(false)]
        internal protected override ICompileSyntax ToCompileSyntax { get { return this; } }

        internal protected override IParsedSyntax CreateSyntax(Token token, IParsedSyntax right) { return new ExpressionSyntax(this, token, ToCompiledSyntaxOrNull(right)); }
    }

}