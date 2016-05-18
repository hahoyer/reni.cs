using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
            , ISymbolProvider<DumpPrintToken>
            , ISymbolProvider<Star>
            , ISymbolProvider<Slash>
            , ISymbolProvider<Mutable>
            , ISymbolProvider<ArrayReference>
    {
        public TypeType(TypeBase value)
        {
            Value = value;
            StopByObjectIds(-61);
        }

        [DisableDump]
        internal override Root Root => Value.Root;

        [DisableDump]
        internal override bool Hllw => true;

        [DisableDump]
        internal TypeBase Value { get; }

        internal override string DumpPrintText => "(" + Value.DumpPrintText + "()) type";

        IImplementation ISymbolProvider<DumpPrintToken>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        IImplementation ISymbolProvider<Star>.Feature
            (Star tokenClass)
            => Feature.Extension.MetaFeature(StarResult);

        IImplementation ISymbolProvider<Slash>.Feature
            (Slash tokenClass)
            => Feature.Extension.MetaFeature(SlashResult);

        IImplementation ISymbolProvider<Mutable>.Feature
            (Mutable tokenClass)
            => Value is ArrayType
                ? Feature.Extension.Value(MutableArrayResult)
                : Value is ArrayReferenceType
                    ? Feature.Extension.Value(MutableReferenceResult)
                    : null;

        IImplementation ISymbolProvider<ArrayReference>.Feature
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
            (Category category, ResultCache left, ContextBase context, Parser.Value right)
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
            (Category category, ResultCache left, ContextBase context, Parser.Value right)
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

            return Root.BitType.Result(category, BitsConst.Convert(count.Value));
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