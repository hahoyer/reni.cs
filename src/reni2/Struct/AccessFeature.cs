using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class AccessFeature
        : DumpableObject
            , IImplementation
            , IValue
            , IConversion
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

        IFunction IEvalImplementation.Function => FunctionFeature.Value;

        IValue IEvalImplementation.Value
        {
            get
            {
                var function = FunctionFeature.Value;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }

        Result IConversion.Execute(Category category)
            => Result(category).ConvertToConverter(View.Type.Pointer);

        TypeBase IConversion.Source => View.Type.Pointer;

        Result IValue.Execute(Category category) => Result(category);

        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(pendingCategory == Category.None)
                return Result(category);
            NotImplementedMethod(category, pendingCategory);
            return null;
        }

        Result Result(Category category) => View.AccessViaObject(category, Position);

        Parser.Syntax Statement => View
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
            return ((IEvalImplementation) valueType.CheckedFeature)?.Function;
        }
    }
}