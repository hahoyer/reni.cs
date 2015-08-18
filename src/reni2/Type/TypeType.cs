using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class TypeType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<Star, IFeatureImplementation>
            , ISymbolProvider<Slash, IFeatureImplementation>
            , ISymbolProvider<Mutable, IFeatureImplementation>
            , ISymbolProvider<ArrayReference, IFeatureImplementation>
    {
        public TypeType(TypeBase value)
        {
            Value = value;
            StopByObjectId(-61);
        }

        [DisableDump]
        internal override Root RootContext => Value.RootContext;

        [DisableDump]
        internal override bool Hllw => true;

        [DisableDump]
        internal TypeBase Value { get; }

        internal override string DumpPrintText => "(" + Value.DumpPrintText + "()) type";

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProvider<Star, IFeatureImplementation>.Feature
            (Star tokenClass)
            => Feature.Extension.MetaFeature(StarResult);

        IFeatureImplementation ISymbolProvider<Slash, IFeatureImplementation>.Feature
            (Slash tokenClass)
            => Feature.Extension.MetaFeature(SlashResult);

        IFeatureImplementation ISymbolProvider<Mutable, IFeatureImplementation>.Feature
            (Mutable tokenClass)
            => Value is ArrayType
                ? Feature.Extension.Value(MutableArrayResult)
                : Value is ArrayReferenceType
                    ? Feature.Extension.Value(MutableReferenceResult)
                    : null;

        IFeatureImplementation ISymbolProvider<ArrayReference, IFeatureImplementation>.Feature
            (ArrayReference tokenClass)
            => Value is ArrayType ? Feature.Extension.Value(ArrayReferenceResult) : null;

        protected override string GetNodeDump() => "(" + Value.NodeDump + ") type";

        internal override Result InstanceResult
            (Category category, Func<Category, Result> getRightResult)
            => RawInstanceResult(category.Typed, getRightResult).LocalReferenceResult;

        Result RawInstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            if(category <= Category.Type.Replenished)
                return Value.Result(category.Typed);
            var constructorResult = Value
                .ConstructorResult(category, getRightResult(Category.Type).Type);
            return constructorResult
                .ReplaceArg(getRightResult);
        }

        new Result DumpPrintTokenResult(Category category)
            => Value.DumpPrintTypeNameResult(category);

        Result StarResult
            (Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var countResult = right.Result(context).AutomaticDereferenceResult;
            var count = countResult
                .Evaluate(context.RootContext.ExecutionContext)
                .ToInt32();
            var type = Value
                .Align
                .Array(count)
                .TypeType;
            return type.Result(category);
        }

        Result SlashResult
            (Category category, ResultCache left, ContextBase context, CompileSyntax right)
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

        Result MutableArrayResult(Category category)
            => ((ArrayType) Value).Mutable.TypeType.Result(category);
        Result ArrayReferenceResult(Category category)
            => ((ArrayType) Value).Reference(true).TypeType.Result(category);
        Result ArrayAccessResult(Category category)
            => ((ArrayType) Value).ElementType.TypeType.Result(category);
        Result MutableReferenceResult(Category category)
            => ((ArrayReferenceType) Value).Mutable.TypeType.Result(category);
    }
}