#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
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

        [NotNull]
        [Node]
        internal readonly CompileSyntax Else;

        protected CondSyntax(CompileSyntax condSyntax, TokenData thenToken, CompileSyntax thenSyntax,
            CompileSyntax elseSyntax)
            : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax ?? new EmptyList(thenToken, thenToken);
        }

        internal override Result ObtainResult(ContextBase context, Category category) { return InternalResult(context, category); }

        Result CondResult(ContextBase context, Category category)
        {
            return Cond
                .Result(context, category.Typed)
                .Conversion(TypeBase.Bit)
                .LocalBlock(category.Typed)
                .ObviousExactConversion(TypeBase.Bit);
        }

        Result ElseResult(ContextBase context, Category category) { return BranchResult(context, category, Else); }
        Result ThenResult(ContextBase context, Category category) { return BranchResult(context, category, Then); }

        Result BranchResult(ContextBase context, Category category, CompileSyntax syntax)
        {
            var branchResult = syntax
                .Result(context, category.Typed)
                .AutomaticDereferenceResult();

            var commonType = CommonType(context);
            return branchResult.Type
                .Conversion(category.Typed, commonType)
                .ReplaceArg(branchResult)
                .LocalBlock(category.Typed)
                .ObviousExactConversion(commonType)
                & category;
        }

        Result InternalResult(ContextBase context, Category category)
        {
            var commonType = CommonType(context);
            if(category <= (Category.Type.Replenished))
                return commonType.Result(category);

            var branchCategory = category & Category.Code.Replenished;
            var condResult = CondResult(context, category);
            var thenResult = ThenResult(context, branchCategory);
            var elseResult = ElseResult(context, branchCategory);
            return commonType
                .Result
                (category
                 , () => condResult.Code.ThenElse(thenResult.Code, elseResult.Code)
                 , () => condResult.CodeArgs + thenResult.CodeArgs + elseResult.CodeArgs
                );
        }

        TypeBase CommonType(ContextBase context)
        {
            var pendingContext = context as PendingContext;
            if(pendingContext == null)
                return Then.Type(context).CommonType(Else.Type(context)).UniqueAlign;

            var parent = pendingContext.Parent;
            return CommonPendingType(parent).UniqueAlign;
        }

        TypeBase CommonPendingType(ContextBase context)
        {
            if(!context.PendingCategory(this).HasType)
            {
                NotImplementedMethod(context);
                return null;
            }

            if(!context.PendingCategory(Then).HasType)
                return Then.Type(context);
            if(!context.PendingCategory(Else).HasType)
                return Else.Type(context);

            NotImplementedMethod(context);
            return null;
        }

        internal override string DumpShort() { return "(" + Cond.DumpShort() + ")then(" + Then.DumpShort() + ")"; }
    }

    [Serializable]
    sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(CompileSyntax condSyntax, TokenData thenToken, CompileSyntax thenSyntax)
            : base(condSyntax, thenToken, thenSyntax, null) { }

        internal override ParsedSyntax CreateElseSyntax(TokenData token, CompileSyntax elseSyntax) { return new ThenElseSyntax(Cond, Token, Then, token, elseSyntax); }
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