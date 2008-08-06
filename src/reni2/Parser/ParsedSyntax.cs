using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Parser
{
    [Serializable]
    internal abstract class ParsedSyntax : ReniObject, IParsedSyntax
    {
        private static bool _isInDump;
        internal readonly Token Token;

        protected ParsedSyntax(Token token)
        {
            Token = token;
        }

        protected ParsedSyntax(Token token, int nextObjectId) : base(nextObjectId)
        {
            Token = token;
        }

        [DumpData(false)]
        public new string NodeDump { get { return base.NodeDump + " " + DumpShort(); } }

        public override sealed string Dump()
        {
            var isInContainerDump = Container._isInDump;
            Container._isInDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = DumpShort();
            if(!isInDump)
                result += FilePosition();
            if(!isInContainerDump)
                result += "\n" + base.Dump();
            Container._isInDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        string IParsedSyntax.DumpShort()
        {
            return DumpShort();
        }

        IParsedSyntax IParsedSyntax.SurroundedByParenthesis(Token token)
        {
            return SurroundedByParenthesis(token);
        }

        IParsedSyntax IParsedSyntax.CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return CreateDeclarationSyntax(token, right);
        }

        [DumpData(false)]
        ICompileSyntax IParsedSyntax.ToCompileSyntax { get { return ToCompileSyntax; } }

        [DumpData(false)]
        internal protected virtual ICompileSyntax ToCompileSyntax
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        IParsedSyntax IParsedSyntax.CreateThenSyntax(Token token, ICompileSyntax condition)
        {
            return CreateThenSyntax(token, condition);
        }

        IParsedSyntax IParsedSyntax.CreateSyntax(Token token, IParsedSyntax right)
        {
            return CreateSyntax(token, right);
        }

        IParsedSyntax IParsedSyntax.CreateElseSyntax(Token token, ICompileSyntax elseSyntax)
        {
            return CreateElseSyntax(token, elseSyntax);
        }

        internal protected virtual IParsedSyntax CreateThenSyntax(Token token, ICompileSyntax condition)
        {
            return new ThenSyntax(condition, token, ToCompileSyntax);
        }

        internal protected virtual IParsedSyntax CreateElseSyntax(Token token, ICompileSyntax elseSyntax)
        {
            NotImplementedMethod(token, elseSyntax);
            return null;
        }

        internal protected virtual string DumpShort()
        {
            return Token.Name;
        }

        internal protected virtual IParsedSyntax CreateListSyntax(Token token, IParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        internal protected virtual IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        internal protected virtual IParsedSyntax SurroundedByParenthesis(Token token)
        {
            NotImplementedMethod(token);
            return null;
        }

        internal protected virtual IParsedSyntax CreateSyntax(Token token, IParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        internal protected virtual string FilePosition()
        {
            return Token.FilePosition;
        }

        internal static ICompileSyntax ToCompiledSyntax(IParsedSyntax parsedSyntax)
        {
            Tracer.Assert(parsedSyntax != null);
            return parsedSyntax.ToCompileSyntax;
        }

        internal static void IsNull(IParsedSyntax parsedSyntax)
        {
            Tracer.Assert(parsedSyntax == null);
        }

        internal static void IsNotNull(IParsedSyntax parsedSyntax)
        {
            Tracer.Assert(parsedSyntax != null);
        }

        internal static ICompileSyntax ToCompiledSyntaxOrNull(IParsedSyntax parsedSyntax)
        {
            if(parsedSyntax == null)
                return null;
            return parsedSyntax.ToCompileSyntax;
        }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }
    }
}