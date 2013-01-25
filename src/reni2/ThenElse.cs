#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

        protected CondSyntax
            (
            CompileSyntax condSyntax,
            TokenData thenToken,
            CompileSyntax thenSyntax,
            CompileSyntax elseSyntax)
            : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax ?? new EmptyList(thenToken, thenToken);
        }

        internal override Result ObtainResult(ContextBase context, Category category) { return InternalResult(context, category); }
        [DisableDump]
        protected override ParsedSyntaxBase[] Children { get { return new ParsedSyntaxBase[] { Cond, Then, Else }; } }

        Result CondResult(ContextBase context, Category category)
        {
            return Cond
                .Result(context, category.Typed)
                .Conversion(context.RootContext.BitType)
                .LocalBlock(category.Typed)
                .ObviousExactConversion(context.RootContext.BitType);
        }

        Result ElseResult(ContextBase context, Category category) { return BranchResult(context, category, Else); }
        Result ThenResult(ContextBase context, Category category) { return BranchResult(context, category, Then); }

        Result BranchResult(ContextBase context, Category category, CompileSyntax syntax)
        {
            var branchResult = syntax
                .Result(context, category.Typed).AutomaticDereferenceResult;

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
                (
                    category
                    ,
                    () => condResult.Code.ThenElse(thenResult.Code, elseResult.Code)
                    ,
                    () => condResult.CodeArgs + thenResult.CodeArgs + elseResult.CodeArgs
                );
        }

        TypeBase CommonType(ContextBase context)
        {
            return Then
                .Type(context)
                .CommonType(Else.Type(context))
                .UniqueAlign;
        }

        protected override string GetNodeDump() { return "(" + Cond.NodeDump + ")then(" + Then.NodeDump + ")"; }
    }

    [Serializable]
    sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(CompileSyntax condSyntax, TokenData thenToken, CompileSyntax thenSyntax)
            : base(condSyntax, thenToken, thenSyntax, null) { }

        internal override ParsedSyntax CreateElseSyntax(TokenData token, CompileSyntax elseSyntax) { return new ThenElseSyntax(Cond, Token, Then, token, elseSyntax); }

        internal override Result ObtainPendingResult(ContextBase context, Category category)
        {
            return context
                .RootContext
                .VoidResult(category);
        }
    }

    [Serializable]
    sealed class ThenElseSyntax : CondSyntax
    {
        [Node]
        readonly TokenData _elseToken;

        public ThenElseSyntax
            (
            CompileSyntax condSyntax,
            TokenData thenToken,
            CompileSyntax thenSyntax,
            TokenData elseToken,
            CompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax, elseSyntax) { _elseToken = elseToken; }

        protected override string GetNodeDump()
        {
            return base.GetNodeDump() + "else(" +
                Else.NodeDump + ")";
        }
    }
}