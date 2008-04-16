using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Tokens that can used in definitions (not reserved tokens)
    /// </summary>
    internal abstract class Defineable : Base
    {
        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        [DumpData(false)]
        internal virtual string CSharpNameOfDefaultOperation
        {
            get
            {
                NotImplementedMethod();
                return "";
            }
        }

        /// <summary>
        /// Gets the name of function defined in class data for C# code generation.for odd sized numbers
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        [DumpExcept(false)]
        internal string DataFunctionName { get { return GetType().Name; } }

        [DumpExcept(false)]
        internal protected virtual bool IsBitSequencePrefixOperation { get { return false; } }

        [DumpExcept(false)]
        internal virtual bool IsCompareOperator { get { return false; } }

        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">Size of the obj.</param>
        /// <param name="argSize">Size of the arg.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        internal virtual Type.Base BitSequenceOperationResultType(int objSize, int argSize)
        {
            NotImplementedMethod(objSize, argSize);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            if(left != null)
                return left.CreateDefinableSyntax(new DefineableToken(token), right);
            if(right != null)
                return new Statement(new MemberElem(new DefineableToken(token), right));
            return new DefinableTokenSyntax(token);
        }

        internal virtual SearchResult SearchFromSequence()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual SearchResult SequenceOfBitSearchResult { get { return null; } }
    }

    abstract internal class SequenceOfBitOperation : Defineable
    {
        readonly Bit.SequenceSearchResult _sequenceOfBitSearchResult;

        protected SequenceOfBitOperation()
        {
            _sequenceOfBitSearchResult = new Bit.SequenceSearchResult(this);
        }

        sealed internal override SearchResult SequenceOfBitSearchResult { get { return _sequenceOfBitSearchResult; } }
    }

    internal class DefinableTokenSyntax : Syntax.Base
    {
        private readonly Token _token;
        private Syntax.Base _declarationSyntax;

        public DefinableTokenSyntax(Token token)
        {
            _token = token;
        }

        private Syntax.Base DeclarationSyntax
        {
            get
            {
                if(_declarationSyntax == null)
                    _declarationSyntax = CreateDeclarationSyntax();
                return _declarationSyntax;
            }
        }

        public string Name { get { return _token.Name; } }

        internal override Result VirtVisit(Context.Base context, Category category)
        {
            return DeclarationSyntax.Visit(context, category);
        }

        internal override Syntax.Base CreateDefinableSyntax(DefineableToken defineableToken, Syntax.Base right)
        {
            return new Statement(CreateMemberElem(), new MemberElem(defineableToken, right));
        }

        public override Syntax.Base CreateDeclarationSyntax(Token token, Syntax.Base right)
        {
            return new DeclarationSyntax(CreateDefineableToken(), token, right);
        }

        private Syntax.Base CreateDeclarationSyntax()
        {
            return new Statement(CreateMemberElem());
        }

        private MemberElem CreateMemberElem()
        {
            return new MemberElem(CreateDefineableToken(), null);
        }

        private DefineableToken CreateDefineableToken()
        {
            return new DefineableToken(_token);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return _token.Name;
        }
    }
}