using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    abstract class SetterTargetType
        : TypeBase
            , IProxyType
            , IConversion
            , IValue
            , IReference
            , ISymbolProvider<ReassignToken>
    {
        readonly int _order;

        protected SetterTargetType() { _order = CodeArgs.NextOrder++; }

        int IContextReference.Order => _order;
        IConversion IProxyType.Converter => this;
        bool IReference.IsWeak => true;
        IConversion IReference.Converter => this;
        Result IValue.Execute(Category category) => GetterResult(category);

        TypeBase IConversion.Source => (TypeBase) this;

        Result IConversion.Execute(Category category)
            => GetterResult(category).ConvertToConverter(this);

        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        internal override bool IsPointerPossible => false;

        IImplementation ISymbolProvider<ReassignToken>.Feature(ReassignToken tokenClass)
            => IsMutable ? Feature.Extension.FunctionFeature(ReassignResult) : null;

        [EnableDumpExcept(false)]
        protected abstract bool IsMutable { get; }

        Result ReassignResult(Category category, TypeBase right)
        {
            if(category == Category.Type)
                return Root.VoidType.Result(category);

            var trace = ObjectId == -97 && category.HasCode;
            StartMethodDump(trace, category, right);
            try
            {
                BreakExecution();
                var sourceResult = right
                    .Conversion(category.Typed, ValueType.ForcedPointer);
                Dump("sourceResult", sourceResult);
                BreakExecution();

                var destinationResult = DestinationResult(category.Typed)
                    .ReplaceArg(Result(category.Typed, this));
                Dump("destinationResult", destinationResult);
                BreakExecution();

                var resultForArg = destinationResult + sourceResult;
                Dump("resultForArg", resultForArg);

                var result = SetterResult(category);
                Dump("result", result);
                BreakExecution();

                return ReturnMethodDump(result.ReplaceArg(resultForArg));
            }
            finally
            {
                EndMethodDump();
            }
        }

        [DisableDump]
        internal abstract TypeBase ValueType { get; }
        internal virtual Result DestinationResult(Category category) => ArgResult(category);
        protected abstract Result SetterResult(Category category);
        protected abstract Result GetterResult(Category category);

        [DisableDump]
        internal override Root Root => ValueType.Root;

        [DisableDump]
        internal override TypeBase TypeForTypeOperator => ValueType.TypeForTypeOperator;

        [DisableDump]
        internal override TypeBase ElementTypeForReference => ValueType.ElementTypeForReference;

        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                if(IsMutable)
                    yield return ReassignToken.TokenId;
            }
        }


        protected override IEnumerable<IConversion> RawSymmetricConversions { get { yield break; } }

        [DisableDump]
        protected override IEnumerable<IConversion> StripConversions
        {
            get { yield return Feature.Extension.Conversion(GetterResult); }
        }

        ResultCache.IResultProvider IValue.FindSource(IContextReference ext)
        {
            NotImplementedMethod(ext);
            return null;
        }
    }
}