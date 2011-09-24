//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using Reni.Type;

namespace Reni.ReniParser
{
    [Serializable]
    internal sealed class ExpressionSyntax : CompileSyntax
    {
        [Node]
        private readonly Defineable _tokenClass;

        [Node]
        internal readonly CompileSyntax Left;

        [Node]
        private readonly TokenData _token;

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

        internal override string DumpShort()
        {
            var result = base.DumpShort() + "." + _tokenClass.ObjectId;
            if(Left != null)
                result = "(" + Left.DumpShort() + ")" + result;
            if(Right != null)
                result += "(" + Right.DumpShort() + ")";
            return result;
        }

        protected override TokenData GetFirstToken() { return Left == null ? Token : Left.FirstToken; }

        protected override TokenData GetLastToken() { return Right == null ? Token : Right.LastToken; }

        internal override string DumpPrintText
        {
            get
            {
                var result = base.DumpShort();
                if(Left != null)
                    result = "(" + Left.DumpPrintText + ")" + result;
                if(Right != null)
                    result += "(" + Right.DumpPrintText + ")";
                return result;
            }
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            var trace = ObjectId == -40 && context.ObjectId == 5 && (category.HasSize && category.HasType && !category.HasCode);
            StartMethodDump(trace, context, category);
            try
            {
                BreakExecution();
                if(Left == null && Right != null)
                {
                    var prefixOperationResult = Right.OperationResult<IPrefixFeature>(context, category, _tokenClass);
                    if(prefixOperationResult != null)
                        return ReturnMethodDump(prefixOperationResult);
                }

                var leftCategory = Category.Type;
                if (Right == null)
                    leftCategory |= category;
                var suffixOperationResult =
                    Left == null
                        ? context.ContextOperationResult(leftCategory, _tokenClass)
                        : Left.OperationResult<ISuffixFeature>(context, leftCategory, _tokenClass);

                if(suffixOperationResult == null)
                {
                    NotImplementedMethod(context, category);
                    return null;
                }

                Tracer.Assert(suffixOperationResult.CompleteCategory == leftCategory);
                Dump("suffixOperationResult", suffixOperationResult);
                BreakExecution();
                var metaFeature = suffixOperationResult.Type.MetaFeature;
                if(metaFeature != null)
                {
                    Dump("metaFeature", metaFeature);
                    BreakExecution();
                    return ReturnMethodDump(metaFeature.ObtainResult(category, context, Left, Right, context.RefAlignParam), true);
                }

                if(Right == null)
                    return ReturnMethodDump(suffixOperationResult, true);

                var functionalFeature = suffixOperationResult.Type.FunctionalFeature;
                var rightResult = Right.Result(context, category.Typed).LocalReferenceResult(context.RefAlignParam);
                Dump("rightResult", rightResult);
                BreakExecution();
                var leftResult = new ResultCache(c=>context.ObjectResult(c,Left));
                leftResult.FunctionDump = "context=" + context.Dump();
                if(Left != null)
                    leftResult.FunctionDump += "\nLeft=" + Left.Dump();
                var result = functionalFeature
                    .ObtainApplyResult(category, leftResult, rightResult, context.RefAlignParam);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}