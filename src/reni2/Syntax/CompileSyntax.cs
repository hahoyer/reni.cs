using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Syntax
{
    [Serializable]
    internal abstract class CompileSyntax : ReniParser.ParsedSyntax, ICompileSyntax
    {
        // Used for debug only
        [IsDumpEnabled(false), Node("Cache")]
        internal readonly DictionaryEx<ContextBase, object> ResultCache = new DictionaryEx<ContextBase, object>();

        internal CompileSyntax(TokenData token)
            : base(token) { }

        internal CompileSyntax(TokenData token, int objectId)
            : base(token, objectId) { }

        string ICompileSyntax.DumpShort() { return DumpShort(); }
        string ICompileSyntax.FilePosition() { return FilePosition(); }
        void ICompileSyntax.AddToCacheForDebug(ContextBase context, object cacheItem) { ResultCache.Add(context, cacheItem); }
        TokenData ICompileSyntax.FirstToken { get { return GetFirstToken(); } }
        TokenData ICompileSyntax.LastToken { get { return GetLastToken(); } }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            return Result(context, category);
        }

        internal protected virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal override ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData token, TokenData rightToken) { return this; }

        internal override ICompileSyntax ToCompiledSyntax() { return this; }
        internal override ReniParser.ParsedSyntax CreateSyntaxOrDeclaration(Defineable tokenClass, TokenData token, ReniParser.ParsedSyntax right) { return new ExpressionSyntax(tokenClass, this, token, right.ToCompiledSyntaxOrNull()); }
    }

}