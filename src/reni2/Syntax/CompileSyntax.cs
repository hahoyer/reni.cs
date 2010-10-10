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
        [IsDumpEnabled(false), Node("Cache")]
        internal readonly DictionaryEx<ContextBase, object> ResultCache = new DictionaryEx<ContextBase, object>();

        internal CompileSyntax(Token token)
            : base(token) { }

        internal CompileSyntax(Token token, int objectId)
            : base(token, objectId) { }

        string ICompileSyntax.DumpShort() { return DumpShort(); }
        string ICompileSyntax.FilePosition() { return FilePosition(); }
        void ICompileSyntax.AddToCacheForDebug(ContextBase context, object cacheItem) { ResultCache.Add(context, cacheItem); }
        Token ICompileSyntax.FirstToken { get { return GetFirstToken(); } }
        Token ICompileSyntax.LastToken { get { return GetLastToken(); } }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            return Result(context, category);
        }

        internal protected virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        protected override IParsedSyntax SurroundedByParenthesis(Token token, Token rightToken) { return this; }

        protected override ICompileSyntax ToCompiledSyntax() { return this; }
        protected override IParsedSyntax CreateSyntaxOrDeclaration(Token token, IParsedSyntax right) { return new ExpressionSyntax(this, token, right.ToCompiledSyntaxOrNull()); }
    }

}