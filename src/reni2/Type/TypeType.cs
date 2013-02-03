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
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.Struct;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.Type
{
    [Serializable]
    sealed class TypeType : TypeBase
        , IFeaturePath<ISuffixFeature, DumpPrintToken>
        , IFeaturePath<ISuffixFeature, Slash>
        , IFeaturePath<ISuffixFeature, Star>
        , IFeaturePath<ISuffixFeature, SequenceToken>

    {
        readonly TypeBase _value;

        public TypeType(TypeBase value)
        {
            _value = value;
            StopByObjectId(61);
        }

        ISuffixFeature IFeaturePath<ISuffixFeature, DumpPrintToken>.GetFeature(DumpPrintToken target) { return Extension.Feature(DumpPrintTokenResult); }
        ISuffixFeature IFeaturePath<ISuffixFeature, Slash>.GetFeature(Slash target) { return Extension.Feature(SlashResult); }
        ISuffixFeature IFeaturePath<ISuffixFeature, Star>.GetFeature(Star target) { return Extension.Feature(StarResult); }
        ISuffixFeature IFeaturePath<ISuffixFeature, SequenceToken>.GetFeature(SequenceToken target)
        {
            var value = Value as ArrayType;
            return value == null ? null : Extension.Feature(value.SequenceTypeResult);
        }

        [DisableDump]
        internal override Root RootContext { get { return _value.RootContext; } }
        [DisableDump]
        internal override bool IsDataLess { get { return true; } }
        [DisableDump]
        internal TypeBase Value { get { return _value; } }

        internal override string DumpPrintText { get { return "(" + Value.DumpPrintText + "()) type"; } }

        protected override string GetNodeDump() { return "(" + Value.NodeDump + ") type"; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, null);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal override Result InstanceResult(Category category, Func<Category, Result> getRightResult) { return RawInstanceResult(category.Typed, getRightResult).LocalPointerKindResult & category; }

        Result RawInstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            if(category <= Category.Type.Replenished)
                return Value.Result(category.Typed);
            return Value
                .ConstructorResult(category, getRightResult(Category.Type).Type)
                .ReplaceArg(getRightResult);
        }

        internal Result DumpPrintTokenResult(Category category) { return Value.DumpPrintTypeNameResult(category); }

        internal Result StarResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var trace = ObjectId == -54 && context.ObjectId == 20 && left.ObjectId == 225;
            StartMethodDump(trace, context, category, left, right);
            try
            {
                var countResult = right.Result(context).AutomaticDereferenceResult;

                Dump("countResult", countResult);
                BreakExecution();

                var count = countResult
                    .Evaluate(context.RootContext.ExecutionContext)
                    .ToInt32();

                Dump("count", count);
                BreakExecution();

                var type = Value
                    .UniqueAlign
                    .UniqueArray(count)
                    .UniqueTypeType;

                Dump("type", type);
                BreakExecution();

                return ReturnMethodDump(type.Result(category));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result SlashResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var rightType = right
                .Type(context)
                .SmartUn<FunctionType>()
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

            return RootContext.BitType.Result(category, BitsConst.Convert(count.Value));
        }
    }
}