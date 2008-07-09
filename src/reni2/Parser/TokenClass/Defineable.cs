using System;
using HWClassLibrary.Debug;
using Reni.Feature;
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

        internal virtual SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>> SearchPrefixFromSequenceOfBit()
        {
            return SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>>.Failure(this);
        }

        internal virtual SearchResult<IContextFeature> SearchContext()
        {
            return SearchResult<IContextFeature>.Failure(this);
        }

        internal virtual SearchResult<IFeatureForSequence> SearchForSequence()
        {
            return SearchResult<IFeatureForSequence>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IFeature, Ref>> SearchFromRef()
        {
            return SearchResult<IConverter<IFeature, Ref>>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IFeature, AssignableRef>> SearchFromAssignableRef()
        {
            return SearchResult<IConverter<IFeature, AssignableRef>>.Failure(this);
        }

        internal virtual SearchResult<IConverter<IConverter<IFeature, Ref>, Struct.Type>> SearchFromRefToStruct()
        {
            return SearchResult<IConverter<IConverter<IFeature, Ref>, Struct.Type>>.Failure(this);
        }

    }
}
