using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Parser
{
    [Serializable]
    internal abstract class ParsedSyntax : ReniObject, IParsedSyntax
    {
        private static bool _isInDump;
        internal readonly Token Token;

        [UsedImplicitly]
        internal static bool IsDetailedDumpRequired = true;

        protected ParsedSyntax(Token token) { Token = token; }

        protected ParsedSyntax(Token token, int nextObjectId)
            : base(nextObjectId) { Token = token; }

        [IsDumpEnabled(false), UsedImplicitly]
        public new string NodeDump { get { return base.NodeDump + " " + DumpShort(); } }

        public override sealed string Dump()
        {
            var isInContainerDump = Container.IsInContainerDump;
            Container.IsInContainerDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = DumpShort();
            if(!IsDetailedDumpRequired)
                return result;
            if(!isInDump)
                result += FilePosition();
            if(!isInContainerDump)
                result += "\n" + base.Dump();
            Container.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        string IParsedSyntax.DumpShort() { return DumpShort(); }

        Token IParsedSyntax.Token { get { return Token; } }
        Token IParsedSyntax.FirstToken { get { return GetFirstToken(); } }
        Token IParsedSyntax.LastToken { get { return GetLastToken(); } }

        protected virtual Token GetFirstToken()
        {
            NotImplementedMethod();
            return null;
        }

        protected virtual Token GetLastToken()
        {
            NotImplementedMethod();
            return null;
        }

        IParsedSyntax IParsedSyntax.SurroundedByParenthesis(Token leftToken, Token rightToken) { return SurroundedByParenthesis(leftToken, rightToken); }

        IParsedSyntax IParsedSyntax.CreateDeclarationSyntax(Token token, IParsedSyntax right) { return CreateDeclarationSyntax(token, right); }

        ICompileSyntax IParsedSyntax.ToCompiledSyntax() { return ToCompiledSyntax(); }

        protected virtual ICompileSyntax ToCompiledSyntax()
        {
            NotImplementedMethod();  //Probably it's a missing right parenthesis
            return null;
        }

        IParsedSyntax IParsedSyntax.CreateThenSyntax(Token token, ICompileSyntax condition) { return CreateThenSyntax(token, condition); }

        IParsedSyntax IParsedSyntax.CreateSyntaxOrDeclaration(Token token, IParsedSyntax right) { return CreateSyntaxOrDeclaration(token, right); }

        IParsedSyntax IParsedSyntax.CreateElseSyntax(Token token, ICompileSyntax elseSyntax) { return CreateElseSyntax(token, elseSyntax); }
        IParsedSyntax IParsedSyntax.RightPar(Token token) { return RightPar(token); }

        protected virtual IParsedSyntax RightPar(Token token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual IParsedSyntax CreateThenSyntax(Token token, ICompileSyntax condition) { return new ThenSyntax(condition, token, ToCompiledSyntax()); }

        protected virtual IParsedSyntax CreateElseSyntax(Token token, ICompileSyntax elseSyntax)
        {
            NotImplementedMethod(token, elseSyntax);
            return null;
        }

        protected internal virtual string DumpShort() { return Token.Name; }

        protected virtual IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual IParsedSyntax SurroundedByParenthesis(Token leftToken, Token rightToken)
        {
            NotImplementedMethod(leftToken, rightToken);  //Probably it's a missing right parenthesis
            return null;
        }

        protected virtual IParsedSyntax CreateSyntaxOrDeclaration(Token token, IParsedSyntax right)
        {
            return new ExpressionSyntax(ToCompiledSyntax(), token, right.ToCompiledSyntaxOrNull());
        }

        protected virtual string FilePosition() { return Token.FilePosition; }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }
    }

    internal static class ParsedSyntaxExtender
    {
        internal static ICompileSyntax CheckedToCompiledSyntax(this IParsedSyntax parsedSyntax)
        {
            parsedSyntax.AssertIsNotNull();
            return parsedSyntax.ToCompiledSyntax();
        }

        internal static void AssertIsNull(this IParsedSyntax parsedSyntax) { Tracer.Assert(parsedSyntax == null); }

        internal static void AssertIsNotNull(this IParsedSyntax parsedSyntax) { Tracer.Assert(parsedSyntax != null); }

        internal static ICompileSyntax ToCompiledSyntaxOrNull(this IParsedSyntax parsedSyntax)
        {
            if(parsedSyntax == null)
                return null;
            return parsedSyntax.ToCompiledSyntax();
        }
    }
}