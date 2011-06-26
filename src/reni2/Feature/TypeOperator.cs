using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    internal sealed class TypeOperator : Special, ISuffix, IInfix
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            if(right == null)
                return new SuffixSyntax(token, left.CheckedToCompiledSyntax(), this);
            return new InfixSyntax(token, left.CheckedToCompiledSyntax(), this, right.CheckedToCompiledSyntax());
        }

        Result ISuffix.Result(ContextBase context, Category category, ICompileSyntax left)
        {
            var result = TypeBase.VoidResult(category).Clone();
            if(category.HasType)
            {
                result.Type = context
                    .Type(left)
                    .TypeForTypeOperator()
                    .TypeType;
            }
            return result;
        }

        Result IInfix.Result(ContextBase context, Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var leftType = context.Type(left).AutomaticDereference();
            if(category.HasCode || category.HasRefs)
                return context.ResultAsReference(category | Category.Type, right).ConvertTo(leftType) & category;
            return leftType.Result(category);
        }
    }
}