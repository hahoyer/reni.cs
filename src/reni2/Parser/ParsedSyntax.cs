using HWClassLibrary.Debug;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Parser
{
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

        /// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns></returns>
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
        internal virtual protected ICompileSyntax ToCompileSyntax
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

        internal virtual protected IParsedSyntax CreateThenSyntax(Token token, ICompileSyntax condition)
        {
            return new ThenElse(condition, token, ToCompileSyntax);
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
    }
}