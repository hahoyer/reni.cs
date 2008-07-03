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
    internal abstract class Defineable : TokenClassBase
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

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            if(left == null && right == null)
                return new DefinableTokenSyntax(token);
            return new ExpressionSyntax(ParsedSyntax.ToCompiledSyntaxOrNull(left), token, ParsedSyntax.ToCompiledSyntaxOrNull(right));
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

        internal virtual SearchResult<IStructFeature> SearchFromStruct()
        {
            return SearchResult<IStructFeature>.Failure(this);
        }
    }
}