using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
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
                                                                    Token token) { return new DeclarationPartSyntax(extensionSyntax, token); }

        internal SearchResult<IFeature> Search() { return SearchResult<IFeature>.SuccessIfMatch(this); }
        internal SearchResult<IPrefixFeature> SearchPrefix() { return SearchResult<IPrefixFeature>.SuccessIfMatch(this); }
        internal SearchResult<IContextFeature> SearchContext() { return SearchResult<IContextFeature>.SuccessIfMatch(this); }

        internal SearchResult<IConverter<IFeature, Sequence>> SearchFromSequenceElement() { return SearchResult<IConverter<IFeature, Sequence>>.SuccessIfMatch(this); }
        internal SearchResult<IConverter<IPrefixFeature, Sequence>> SearchPrefixFromSequenceElement() { return SearchResult<IConverter<IPrefixFeature, Sequence>>.SuccessIfMatch(this); }
        internal SearchResult<IConverter<IFeature, Sequence>> SearchForSequence() { return SearchResult<IConverter<IFeature, Sequence>>.SuccessIfMatch(this); }

        internal SearchResult<IConverter<IConverter<IFeature, Sequence>, Bit>> SearchFromSequenceOfBit() { return SearchResult<IConverter<IConverter<IFeature, Sequence>, Bit>>.SuccessIfMatch(this); }
        internal SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>> SearchPrefixFromSequenceOfBit() { return SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>>.SuccessIfMatch(this); }


        internal SearchResult<IConverter<IFeature, Ref>> SearchFromRef() { return SearchResult<IConverter<IFeature, Ref>>.SuccessIfMatch(this); }

        internal SearchResult<IConverter<IFeature, AssignableRef>> SearchFromAssignableRef() { return SearchResult<IConverter<IFeature, AssignableRef>>.SuccessIfMatch(this); }

        internal SearchResult<IStructFeature> SearchFromStruct() { return SearchResult<IStructFeature>.Failure(this); }

        internal SearchResult<IConverter<IFeature, IArray>> SearchFromArray() { return SearchResult<IConverter<IFeature, IArray>>.SuccessIfMatch(this); }
    }

    internal sealed class DeclarationPartSyntax : ParsedSyntax
    {
        private readonly DefineableToken _defineableToken;
        private readonly DeclarationExtensionSyntax _extensionSyntax;

        internal DeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, Token token)
            : base(token)
        {
            _defineableToken = new DefineableToken(token);
            _extensionSyntax = extensionSyntax;
        }

        internal protected override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right) { return new DeclarationSyntax(_extensionSyntax, _defineableToken, token, right); }
    }
}