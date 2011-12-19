// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;
using Reni.Type;

namespace Reni
{
    [Serializable]
    abstract class CondSyntax : CompileSyntax
    {
        [Node]
        internal readonly CompileSyntax Cond;

        [Node]
        internal readonly CompileSyntax Then;

        [Node]
        internal readonly CompileSyntax Else;

        protected CondSyntax(CompileSyntax condSyntax, TokenData thenToken, CompileSyntax thenSyntax,
            CompileSyntax elseSyntax)
            : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        internal override Result ObtainResult(ContextBase context, Category category) { return InternalResult(context, category); }

        internal Result CondResult(ContextBase context, Category category)
        {
            return Cond
                .Result(context, category.Typed)
                .Conversion(TypeBase.Bit)
                .Align(context.AlignBits)
                .LocalBlock(category.Typed)
                .Conversion(TypeBase.Bit);
        }

        Result ElseResult(ContextBase context, Category category)
        {
            if(Else == null)
                return TypeBase.Void.Result(category);
            return CondBranchResult(context, category, Else);
        }

        Result ThenResult(ContextBase context, Category category) { return CondBranchResult(context, category, Then); }

        Result CondBranchResult(ContextBase context, Category category, CompileSyntax syntax)
        {
            var branchResult = syntax.Result(context, category.Typed).AutomaticDereference();
            if((category - Category.Type).IsNone)
                return branchResult.Align(context.RefAlignParam.AlignBits);

            var commonType = context.CommonType(this);
            var result = branchResult.Type
                .Conversion(category.Typed, commonType)
                .ReplaceArg(branchResult);
            return result.LocalBlock(category);
        }

        Result InternalResult(ContextBase context, Category category)
        {
            var commonType = context.CommonType(this);
            if(category <= (Category.Type | Category.Size))
                return commonType.Result(category);

            var condResult = CondResult(context, category);
            return commonType.Result
                (
                    category,
                    () => condResult.Code.ThenElse(ThenResult(context, Category.Code).Code, ElseResult(context, Category.Code).Code),
                    () => condResult.CodeArgs + context.CommonRefs(this)
                );
        }

        internal Result CommonResult(ContextBase context, Category category, bool thenIsPending, bool elseIsPending)
        {
            if(!thenIsPending)
                return ThenResult(context, category);
            if(!elseIsPending)
                return ElseResult(context, category);
            NotImplementedMethod(context, category, thenIsPending, elseIsPending);
            return null;
        }

        internal Result CommonResult(ContextBase context, Category category)
        {
            Tracer.Assert(category <= (Category.Type | Category.CodeArgs));
            var thenResult = ThenResult(context, category);
            var elseResult = ElseResult(context, category);
            var result = new Result
                (category
                 , getType: () => TypeBase.CommonType(thenResult.Type, elseResult.Type)
                 , getArgs: () => thenResult.CodeArgs + elseResult.CodeArgs
                );
            return result;
        }

        internal override string DumpShort() { return "(" + Cond.DumpShort() + ")then(" + Then.DumpShort() + ")"; }
    }

    [Serializable]
    sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(CompileSyntax condSyntax, TokenData thenToken, CompileSyntax thenSyntax)
            : base(condSyntax, thenToken, thenSyntax, null) { }

        internal override ReniParser.ParsedSyntax CreateElseSyntax(TokenData token, CompileSyntax elseSyntax) { return new ThenElseSyntax(Cond, Token, Then, token, elseSyntax); }
    }

    [Serializable]
    sealed class ThenElseSyntax : CondSyntax
    {
        [Node]
        readonly TokenData _elseToken;

        public ThenElseSyntax(CompileSyntax condSyntax, TokenData thenToken, CompileSyntax thenSyntax, TokenData elseToken,
            CompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax, elseSyntax) { _elseToken = elseToken; }

        internal override string DumpShort()
        {
            return base.DumpShort() + "else(" +
                   Else.DumpShort() + ")";
        }
    }
}