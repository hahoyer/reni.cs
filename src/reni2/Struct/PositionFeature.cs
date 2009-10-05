using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal abstract class PositionFeatureBase :
        ReniObject,
        IContextFeature,
        IFeature, ISearchPath<IFeature, Ref>
    {
        protected readonly Context StructContext;

        [DumpData(true)]
        protected readonly int Position;

        protected PositionFeatureBase(Context structContext, int position)
        {
            Position = position;
            StructContext = structContext;
        }

        IFeature ISearchPath<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == StructContext.ForCode.RefAlignParam);
            return this;
        }

        Result IContextFeature.Apply(Category category) { return Apply(category); }

        protected abstract Result Apply(Category category);

        Result IFeature.Apply(Category category, TypeBase objectType)
        {
            NotImplementedMethod(category, objectType);
            return null;
        }

        internal static Result AccessResult(ContextBase callContext, Category category, ICompileSyntax left, int position)
        {
            var objectResult = callContext.ResultAsRef(category | Category.Type, left);
            return objectResult.Type.AccessResultAsArg(category, position).UseWithArg(objectResult);
        }
    }

    [Serializable]
    internal sealed class PositionFeature : PositionFeatureBase
    {
        private readonly PropertyPositionFeature _property;

        internal PositionFeature(EmptyList emptyList, Context structContext, int position)
            : base(
            structContext, position) { _property = new PropertyPositionFeature(emptyList, structContext, position); }

        public PositionFeatureBase ToProperty(bool isPoperty)
        {
            if(isPoperty)
                return _property;
            return this;
        }

        protected override Result Apply(Category category)
        {
            return StructContext.AccessResultAsContextRef(category,Position);
        }
    }

    internal class PropertyPositionFeature : PositionFeatureBase
    {
        private readonly EmptyList _emptyList;

        public PropertyPositionFeature(EmptyList emptyList, Context structContext, int position)
            : base(structContext, position) { _emptyList = emptyList; }

        protected override Result Apply(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
    }
}