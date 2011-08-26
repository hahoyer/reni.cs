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
            var trace = _tokenClass.ObjectId == -24 && Right != null && Right.GetObjectId() == 40 && category.HasIsDataLess;
            StartMethodDump(trace, context, category);
            try
            {
                BreakExecution();
                if(Left == null && Right != null)
                {
                    var prefixOperationResult = OperationResult<IPrefixFeature>(context, category, Right, _tokenClass);
                    if(prefixOperationResult != null)
                        return ReturnMethodDump(prefixOperationResult);
                }

                var suffixOperationResult =
                    Left == null
                        ? context.ContextOperationResult(category.Typed, _tokenClass)
                        : OperationResult<IFeature>(context, category.Typed, Left, _tokenClass);

                if(suffixOperationResult == null)
                {
                    NotImplementedMethod(category, Left, _tokenClass, Right);
                    return null;
                }

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
                Dump("suffixOperationResult", suffixOperationResult);
                Dump("rightResult", rightResult);
                BreakExecution();
                var result = functionalFeature
                    .ObtainApplyResult(category, suffixOperationResult, rightResult, context.RefAlignParam);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        
        private static Result OperationResult<TFeature>(ContextBase context, Category category, CompileSyntax target, Defineable defineable)
            where TFeature : class
        {
            var targetType = target.Type(context);
            var operationResult = targetType.OperationResult<TFeature>(category, defineable, context.RefAlignParam);
            if(operationResult == null)
                return (null);

            var targetResult = target.ResultAsReference(context, category.Typed);
            var result = operationResult.ReplaceArg(targetResult);
            return (result);
        }

    }
}