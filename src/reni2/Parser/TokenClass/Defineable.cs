using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Feature;
using Reni.Parser.TokenClass.Symbol;
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

        internal SearchResult<TFeatureType> SubSearch<TFeatureType, TType>(TType type)
            where TType : IDumpShortProvider
            where TFeatureType : class
        {
            return Check<IConverter<TFeatureType, TType>>().RecordSubTrial(type).Convert(type);
        }

        internal SearchResult<TFeatureType> Check<TFeatureType>() 
            where TFeatureType : class
        {
            return new SearchResult<TFeatureType>(this as TFeatureType, this);
        }
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

        protected internal override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new DeclarationSyntax(_extensionSyntax, _defineableToken, token, right);
        }
    }
}