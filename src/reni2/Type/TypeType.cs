using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.ReniSyntax;
using Reni.Struct;

namespace Reni.Type
{
    sealed class TypeType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<Star, IFeatureImplementation>
    {
        public TypeType(TypeBase value)
        {
            Value = value;
            StopByObjectId(61);
        }

        [DisableDump]
        internal override Root RootContext => Value.RootContext;

        [DisableDump]
        internal override bool Hllw => true;

        [DisableDump]
        internal TypeBase Value { get; }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProvider<Star, IFeatureImplementation>.Feature(Star tokenClass)
            => Extension.MetaFeature(StarResult);
        internal override string DumpPrintText => "(" + Value.DumpPrintText + "()) type";

        protected override string GetNodeDump() => "(" + Value.NodeDump + ") type";

        internal override Result InstanceResult(Category category, Func<Category, Result> getRightResult)
            => RawInstanceResult(category.Typed, getRightResult).LocalReferenceResult & category;

        Result RawInstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            if(category <= Category.Type.Replenished)
                return Value.Result(category.Typed);
            var constructorResult = Value
                .ConstructorResult(category, getRightResult(Category.Type).Type);
            return constructorResult
                .ReplaceArg(getRightResult);
        }

        new Result DumpPrintTokenResult(Category category) => Value.DumpPrintTypeNameResult(category);

        Result StarResult(Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var countResult = right.Result(context).AutomaticDereferenceResult;
            var count = countResult
                .Evaluate(context.RootContext.ExecutionContext)
                .ToInt32();
            var type = Value
                .Align
                .Array(count, false)
                .TypeType;
            return type.Result(category);
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