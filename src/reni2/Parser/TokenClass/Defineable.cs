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
        [DumpData(false)]
        internal virtual string CSharpNameOfDefaultOperation
        {
            get
            {
                NotImplementedMethod();
                return "";
            }
        }

        [DumpExcept(false)]
        internal string DataFunctionName { get { return GetType().Name; } }

        [DumpExcept(false)]
        internal virtual bool IsCompareOperator { get { return false; } }
        [DumpExcept(false)]
        internal abstract string Name { get; }

        internal virtual TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            NotImplementedMethod(objSize, argSize);
            throw new NotImplementedException();
        }

        internal override SyntaxBase CreateSyntax(SyntaxBase left, Token token, SyntaxBase right)
        {
            if(left != null)
                return left.CreateDefinableSyntax(new DefineableToken(token), right);
            if(right != null)
                return new Statement(new MemberElem(new DefineableToken(token), right));
            return new DefinableTokenSyntax(token);
        }

        internal virtual SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Failure(this);
        }

        internal virtual SearchResult<IPrefixFeature> SearchPrefix()
        {
            return SearchResult<IPrefixFeature>.Failure(this);
        }

        internal virtual SearchResult<ISequenceElementFeature> SearchFromSequenceElement()
        {
            return SearchResult<ISequenceElementFeature>.Failure(this);
        }

        internal virtual SearchResult<ISequenceElementPrefixFeature> SearchPrefixFromSequenceElement()
        {
            return SearchResult<ISequenceElementPrefixFeature>.Failure(this);
        }

        internal virtual SearchResult<ISequenceOfBitFeature> SearchFromSequenceOfBit()
        {
            return SearchResult<ISequenceOfBitFeature>.Failure(this);
        }

        internal virtual SearchResult<ISequenceOfBitPrefixFeature> SearchPrefixFromSequenceOfBit()
        {
            return SearchResult<ISequenceOfBitPrefixFeature>.Failure(this);
        }

        internal virtual SearchResult<IContextFeature> SearchContext()
        {
            return SearchResult<IContextFeature>.Failure(this);
        }

        internal virtual SearchResult<IFeatureForSequence> SearchForSequence()
        {
            return SearchResult<IFeatureForSequence>.Failure(this);
        }

        internal virtual SearchResult<IRefFeature> SearchFromRef()
        {
            return SearchResult<IRefFeature>.Failure(this);
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

        internal protected override string FilePosition { get { return _token.FilePosition; } }
    }
}