using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class AccessFeature
        : DumpableObject
            , ITypeImplementation
            , IValue
            , ResultCache.IResultProvider
    {
        static int _nextObjectId;

        internal AccessFeature(CompoundView compoundView, int position)
            : base(_nextObjectId++)
        {
            View = compoundView;
            Position = position;
            FunctionFeature = new ValueCache<IFunction>(ObtainFunctionFeature);
            StopByObjectIds();
        }

        [EnableDump]
        public CompoundView View { get; }
        [EnableDump]
        public int Position { get; }

        ValueCache<IFunction> FunctionFeature { get; }

        IMeta IMetaImplementation.Function
            => (Statement as FunctionSyntax)?.MetaFunctionFeature(View);

        IFunction IImplementation.Function => FunctionFeature.Value;

        IValue IImplementation.Value
        {
            get
            {
                var function = FunctionFeature.Value;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }

        Result IValue.Result(Category category) => Result(category);

        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(pendingCategory == Category.None)
                return Result(category);
            NotImplementedMethod(category, pendingCategory);
            return null;
        }

        object ResultCache.IResultProvider.Target => this;

        Result Result(Category category) => View.AccessViaObject(category, Position);

        TypeBase IValue.TargetType => View.Type;

        CompileSyntax Statement => View
            .Compound
            .Syntax
            .Statements[Position];

        IFunction ObtainFunctionFeature()
        {
            var functionSyntax = Statement as FunctionSyntax;
            if(functionSyntax != null)
                return functionSyntax.FunctionFeature(View);

            var valueType = View.ValueType(Position);
            StopByObjectIds();
            return ((IImplementation) valueType.CheckedFeature)?.Function;
        }
    }
}