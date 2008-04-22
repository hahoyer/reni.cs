using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
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
        internal virtual bool IsCompareOperator { get { return false; } }

        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">Size of the obj.</param>
        /// <param name="argSize">Size of the arg.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        internal virtual TypeBase BitSequenceOperationResultType(int objSize, int argSize)
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
        internal override SyntaxBase CreateSyntax(SyntaxBase left, Token token, SyntaxBase right)
        {
            if(left != null)
                return left.CreateDefinableSyntax(new DefineableToken(token), right);
            if(right != null)
                return new Statement(new MemberElem(new DefineableToken(token), right));
            return new DefinableTokenSyntax(token);
        }

        internal virtual SearchResult<ISequenceFeature> SearchFromSequence()
        {
            return SearchResult<ISequenceFeature>.Failure(this);
        }

        internal virtual SearchResult<ISequencePrefixFeature> SearchPrefixFromSequence()
        {
            return SearchResult<ISequencePrefixFeature>.Failure(this);
        }

        internal virtual SearchResult<ISequenceOfBitFeature> SearchFromSequenceOfBit()
        {
            return SearchResult<ISequenceOfBitFeature>.Failure(this);
        }

        internal virtual SearchResult<ISequenceOfBitPrefixFeature> SearchPrefixFromSequenceOfBit()
        {
            return SearchResult<ISequenceOfBitPrefixFeature>.Failure(this);
        }

        internal virtual SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Failure(this);
        }

        public SearchResult<IPrefixFeature> SearchPrefix()
        {
            return SearchResult<IPrefixFeature>.Failure(this);
        }

        public SearchResult<IContextFeature> SearchContext()
        {
            return SearchResult<IContextFeature>.Failure(this);
        }
    }

    internal class DefinableTokenSyntax : SyntaxBase
    {
        private readonly Token _token;
        private SyntaxBase _declarationSyntax;

        public DefinableTokenSyntax(Token token)
        {
            _token = token;
        }

        private SyntaxBase DeclarationSyntax
        {
            get
            {
                if(_declarationSyntax == null)
                    _declarationSyntax = CreateDeclarationSyntax();
                return _declarationSyntax;
            }
        }

        public string Name { get { return _token.Name; } }

        internal override Result VirtVisit(ContextBase context, Category category)
        {
            return DeclarationSyntax.Visit(context, category);
        }

        internal override SyntaxBase CreateDefinableSyntax(DefineableToken defineableToken, SyntaxBase right)
        {
            return new Statement(CreateMemberElem(), new MemberElem(defineableToken, right));
        }

        public override SyntaxBase CreateDeclarationSyntax(Token token, SyntaxBase right)
        {
            return new DeclarationSyntax(CreateDefineableToken(), token, right);
        }

        private SyntaxBase CreateDeclarationSyntax()
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

        internal override string DumpShort()
        {
            return _token.Name;
        }
    }
}