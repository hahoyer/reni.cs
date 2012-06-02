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

        Result ISuffix.Result(ContextBase context, Category category, CompileSyntax left)
        {
            var result = TypeBase.VoidResult(category).Clone();
            if(category.HasType)
            {
                result.Type = left
                    .Type(context).TypeForTypeOperator
                    .UniqueTypeType;
            }
            return result;
        }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var leftType = left.Type(context).AutomaticDereferenceType;
            if(category.HasCode || category.HasArgs)
                return context.ResultAsReference(category.Typed, right).Conversion(leftType) & category;
            return leftType.Result(category);
        }
    }
}