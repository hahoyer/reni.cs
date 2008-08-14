using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Feature;
using Reni.Parser.TokenClass.Symbol;
using Reni.Struct;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Tokens that can used in definitions (not reserved tokens)
    /// </summary>
    [Serializable]
    internal abstract class Defineable : TokenClassBase
    {
        [Node, DumpData(false)]
        internal virtual string CSharpNameOfDefaultOperation { get { return Name; } }
        [Node]
        internal string DataFunctionName { get { return GetType().Name; } }
        [Node]
        internal virtual bool IsCompareOperator { get { return false; } }

        internal virtual TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            NotImplementedMethod(objSize, argSize);
            throw new NotImplementedException();
        }

        internal virtual TypeBase BitSequenceOperationResultType(int objSize)
        {
            NotImplementedMethod(objSize);
            throw new NotImplementedException();
        }

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            if(left == null && right == null)
                return new DefinableTokenSyntax(token);
            if(left == null)
                return new ExpressionSyntax(null, token, ParsedSyntax.ToCompiledSyntaxOrNull(right));
            return left.CreateSyntax(token, right);
        }

        internal override IParsedSyntax CreateDeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax,
            Token token)
        {
            return new DeclarationPartSyntax(extensionSyntax, token);
        }

        internal virtual SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Failure(this);
        }

        internal virtual SearchResult<IPrefixFeature> SearchPrefix()
        {
            return SearchResult<IPrefixFeature>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IFeature, Sequence>> SearchFromSequenceElement()
        {
            return SearchResult<IConverter<IFeature, Sequence>>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IPrefixFeature, Sequence>> SearchPrefixFromSequenceElement()
        {
            return SearchResult<IConverter<IPrefixFeature, Sequence>>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IConverter<IFeature, Sequence>, Bit>> SearchFromSequenceOfBit()
        {
            return SearchResult<IConverter<IConverter<IFeature, Sequence>, Bit>>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>>
            SearchPrefixFromSequenceOfBit()
        {
            return SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>>.Failure(this);
        }

        internal virtual SearchResult<IContextFeature> SearchContext()
        {
            return SearchResult<IContextFeature>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IFeature, Sequence>> SearchForSequence()
        {
            return SearchResult<IConverter<IFeature, Sequence>>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IFeature, Ref>> SearchFromRef()
        {
            return SearchResult<IConverter<IFeature, Ref>>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IFeature, AssignableRef>> SearchFromAssignableRef()
        {
            return SearchResult<IConverter<IFeature, AssignableRef>>.Failure(this);
        }

        internal virtual SearchResult<StructFeature> SearchFromStruct()
        {
            return SearchResult<StructFeature>.Failure(this);
        }
    }

    internal sealed class DeclarationPartSyntax : ParsedSyntax
    {
        private readonly DefineableToken _defineableToken;
        private readonly DeclarationExtensionSyntax _extensionSyntax;

        internal DeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, Token token) : base(token)
        {
            _defineableToken = new DefineableToken(token);
            _extensionSyntax = extensionSyntax;
        }

        internal protected override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new DeclarationSyntax(_extensionSyntax, _defineableToken, token, right);
        }
    }
}