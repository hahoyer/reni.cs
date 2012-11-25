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

using HWClassLibrary.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    [Serializable]
    sealed class ExpressionSyntax : CompileSyntax
    {
        [Node]
        readonly Defineable _tokenClass;
        [Node]
        internal readonly CompileSyntax Left;
        [Node]
        readonly TokenData _token;
        [Node]
        internal readonly CompileSyntax Right;

        internal ExpressionSyntax(Defineable tokenClass, CompileSyntax left, TokenData token, CompileSyntax right)
            : base(token)
        {
            _tokenClass = tokenClass;
            Left = left;
            _token = token;
            Right = right;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            var result = Result(context, category);
            if(result == null)
                return UndefinedSymbolIssue.Type(context, this).IssueResult(category);

            Tracer.Assert(category <= result.CompleteCategory);
            return result;
        }

        internal Probe[] Probes(ContextBase context)
        {
            var result = new List<Probe>();
            if(Left == null && Right != null)
                result.AddRange(Right.PrefixOperationProbes(context, _tokenClass));

            result.AddRange(Left == null
                                ? context.Probes(_tokenClass)
                                : context
                                      .Type(Left)
                                      .TypeForSearchProbes
                                      .Probes<ISuffixFeature>(_tokenClass, this));
            return result.ToArray();
        }

        new Result Result(ContextBase context, Category category)
        {
            if(Left == null && Right != null)
            {
                var prefixOperationResult = Right.PrefixOperationResult(context, category, _tokenClass);
                if(prefixOperationResult != null)
                    return prefixOperationResult
                        .ReplaceArg(Right.PointerKindResult(context, category));
            }

            var searchResult
                = Left == null
                      ? context.Search(_tokenClass)
                      : context
                            .Type(Left)
                            .TypeForSearchProbes
                            .Search<ISuffixFeature>(_tokenClass, this);
            return searchResult == null ? null : searchResult.FunctionResult(context, category, this);
        }

        protected override string GetNodeDump()
        {
            var result = base.GetNodeDump();
            if(Left != null)
                result = "(" + Left.NodeDump + ")" + result;
            if(Right != null)
                result += "(" + Right.NodeDump + ")";
            return result;
        }
        protected override ParsedSyntaxBase[] Children { get { return new ParsedSyntaxBase[]{Left,Right}; } }

        internal override TokenData FirstToken { get { return Left == null ? Token : Left.FirstToken; } }

        internal override TokenData LastToken { get { return Right == null ? Token : Right.LastToken; } }

        internal override string DumpPrintText
        {
            get
            {
                var result = base.GetNodeDump();
                if(Left != null)
                    result = "(" + Left.DumpPrintText + ")" + result;
                if(Right != null)
                    result += "(" + Right.DumpPrintText + ")";
                return result;
            }
        }
    }

    // Lord of the weed
    // Katava dscho dscho
}