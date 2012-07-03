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
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    sealed class TypeType : TypeBase, IFeature, IFunctionFeature
    {
        readonly TypeBase _value;

        public TypeType(TypeBase value) { _value = value; }

        [DisableDump]
        internal override bool IsDataLess { get { return true; } }
        [DisableDump]
        internal override IFeature Feature { get { return this; } }
        TypeBase Value { get { return _value; } }

        internal override string DumpPrintText { get { return "(" + Value.DumpPrintText + "()) type"; } }

        internal override string DumpShort() { return "(" + Value.DumpShort() + ") type"; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, null);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal Result DumpPrintResult(Category category) { return Value.DumpPrintTypeNameResult(category); }
        
        internal Result Repeat(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var count = right
                .Result(context)
                .AutomaticDereferenceResult()
                .Evaluate(context.RootContext.ExecutionContext)
                .ToInt32();
            return Value
                .UniqueAlign(context.RefAlignParam.AlignBits)
                .UniqueArray(count)
                .UniqueTypeType
                .Result(category);
        }

        internal Result Split(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var rightType = right
                .Type(context)
                .SmartUn<Struct.FunctionType>()
                .SmartUn<PointerType>();
            var rightTypeType = rightType as TypeType;
            if(rightTypeType == null)
            {
                NotImplementedMethod(context, category, left, right, "rightType", rightType);
                return null;
            }

            var count = Value.SmartArrayLength(rightTypeType.Value);
            if(count == null)
            {
                NotImplementedMethod(context, category, left, right, "rightType", rightType);
                return null;
            }

            return Result(category, BitsConst.Convert(count.Value));
        }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return this; } }
        ISimpleFeature IFeature.Simple { get { return null; } }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return Value.ConstructorResult(category, argsType); }

        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return this; } }

        internal Result CreateReference(ContextBase context, Category category, CompileSyntax target)
        {
            var rawResult = Value.SmartReference.ArgResult(category);
            if (category <= Category.Type.Replenished)
                return rawResult;

            var targetResult = target.SmartReferenceResult(context, category.Typed);
            var convertedResult = targetResult.Conversion(Value.SmartReference);
            return rawResult.ReplaceArg(convertedResult);
        }
    }
}