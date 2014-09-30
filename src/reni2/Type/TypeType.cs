using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Type
{
    sealed class TypeType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<Star, IFeatureImplementation>
    {
        readonly TypeBase _value;

        public TypeType(TypeBase value)
        {
            _value = value;
            StopByObjectId(61);
        }

        [DisableDump]
        internal override Root RootContext { get { return _value.RootContext; } }

        [DisableDump]
        internal override bool Hllw { get { return true; } }

        [DisableDump]
        internal TypeBase Value { get { return _value; } }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        IFeatureImplementation ISymbolProvider<Star, IFeatureImplementation>.Feature(Star token) { return Extension.MetaFeature(StarResult); }
        internal override string DumpPrintText { get { return "(" + Value.DumpPrintText + "()) type"; } }

        protected override string GetNodeDump() { return "(" + Value.NodeDump + ") type"; }

        internal override Result InstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            return RawInstanceResult(category.Typed, getRightResult).LocalPointerKindResult & category;
        }

        Result RawInstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            if(category <= Category.Type.Replenished)
                return Value.Result(category.Typed);
            return Value
                .ConstructorResult(category, getRightResult(Category.Type).Type)
                .ReplaceArg(getRightResult);
        }

        internal Result DumpPrintTokenResult(Category category) { return Value.DumpPrintTypeNameResult(category); }

        Result StarResult(ContextBase context, Category category, CompileSyntax right)
        {
            var countResult = right.Result(context).AutomaticDereferenceResult;
            var count = countResult
                .Evaluate(context.RootContext.ExecutionContext)
                .ToInt32();
            var type = Value
                .UniqueAlign
                .UniqueArray(count)
                .UniqueTypeType;
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