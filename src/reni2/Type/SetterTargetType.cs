using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    abstract class SetterTargetType
        : TypeBase
            , IProxyType
            , IConverter
            , IReferenceType
            , IFeatureInheritor
    {
        readonly int _order;

        protected SetterTargetType() { _order = CodeArgs.NextOrder++; }

        Size IContextReference.Size { get { return Size; } }
        int IContextReference.Order { get { return _order; } }
        IConverter IProxyType.Converter { get { return this; } }
        bool IReferenceType.IsWeak { get { return true; } }
        IConverter IReferenceType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return ValueType; } }
        Result IConverter.Result(Category category) { return GetterResult(category); }
        Result IFeatureInheritor.Source(Category category) { return GetterResult(category); }

        internal Result AssignmentResult(Category category, IContextReference objectReference, TypeBase argsType)
        {
            if(category == Category.Type)
                return RootContext.VoidResult(category);

            var trace = ObjectId == -1;
            StartMethodDump(trace, category, argsType);
            try
            {
                BreakExecution();
                var sourceResult = argsType
                    .Conversion(category.Typed, ValueType).LocalPointerKindResult;
                Dump("sourceResult", sourceResult);
                BreakExecution();

                var destinationResult = DestinationResult(category.Typed)
                    .ReplaceArg(Result(category.Typed, objectReference));
                Dump("destinationResult", destinationResult);
                BreakExecution();

                var resultForArg = destinationResult + sourceResult;
                Dump("resultForArg", resultForArg);

                var result = RootContext.VoidType.Result(category, SetterResult);
                Dump("result", result);
                BreakExecution();

                return ReturnMethodDump(result.ReplaceArg(resultForArg));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal abstract TypeBase ValueType { get; }
        internal virtual Result DestinationResult(Category category) { return ArgResult(category); }
        protected abstract Result SetterResult(Category category);
        protected abstract Result GetterResult(Category category);

        [DisableDump]
        internal override Root RootContext { get { return ValueType.RootContext; } }

        internal ResultCache ForceDePointer(Category category)
        {
            var result = GetterResult(category.Typed);
            return result.Type.DePointer(category).Data.ReplaceArg(result);
        }

        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return ValueType.TypeForTypeOperator; } }

        [DisableDump]
        internal override TypeBase ElementTypeForReference { get { return ValueType.ElementTypeForReference; } }

        protected override IEnumerable<ISimpleFeature> ObtainRawReflexiveConversions() { yield break; }

        internal override ISimpleFeature GetStripConversion() { return Extension.SimpleFeature(GetterResult); }
    }
}