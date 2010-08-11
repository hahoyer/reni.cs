using System;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
    internal sealed class TypeOperator : Special, ISuffix, IInfix
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            if (right == null)
                return new SuffixSyntax(token, left.CheckedToCompiledSyntax(), this);
            return new InfixSyntax(token, left.CheckedToCompiledSyntax(), this, right.CheckedToCompiledSyntax());
        }

        Result ISuffix.Result(ContextBase context, Category category, ICompileSyntax left)
        {
            var result = TypeBase.CreateVoidResult(category).Clone();
            if(category.HasType)
                result.Type = context
                    .Type(left)
                    .GetTypeForTypeOperator()
                    .CreateReference(context.RefAlignParam)
                    .TypeType;
            return result;
        }

        Result IInfix.Result(ContextBase context, Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var leftType = context.Type(left).AutomaticDereference();
            if(category.HasCode || category.HasRefs)
                return context.ResultAsRef(category|Category.Type, right).ConvertTo(leftType) & category;
            return leftType.CreateResult(category);
        }
    }
}