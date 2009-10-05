using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

#pragma warning disable 1911

namespace Reni.Struct
{
    [Serializable]
    internal sealed class Type : TypeBase
    {
        private bool _isGetSizeActive;

        [Node]
        internal readonly FullContext Context;

        private readonly ISearchPath<IFeature, Ref> _dumpPrintFromRefFeature;

        private class DumpPrintFromRefFeatureImplementation : ReniObject
                                                              , ISearchPath<IFeature, Ref>, IFeature
        {
            [DumpData(true)]
            private readonly Type _type;

            public DumpPrintFromRefFeatureImplementation(Type type) { _type = type; }

            IFeature ISearchPath<IFeature, Ref>.Convert(Ref @ref) { return this; }

            Result IFeature.Apply(Category category, TypeBase objectType)
            {
                return _type.DumpPrintFromRef(category);
            }
        }

        internal Type(FullContext context)
        {
            _dumpPrintFromRefFeature = new DumpPrintFromRefFeatureImplementation(this);
            Context = context;
        }

        protected override Size GetSize()
        {
            if(_isGetSizeActive)
                return Size.Create(-1);
            _isGetSizeActive = true;
            var result = Context.InternalSize();
            _isGetSizeActive = false;
            return result;
        }

        internal override string DumpShort() { return "type." + ObjectId + "(context." + Context.DumpShort() + ")"; }

        protected internal override int IndexSize { get { return Context.IndexSize; } }

        private List<ICompileSyntax> StatementList { get { return Context.StatementList; } }

        [DumpData(false)]
        internal ISearchPath<IFeature, Ref> DumpPrintFromRefFeature { get { return _dumpPrintFromRefFeature; } }

        internal override Result AccessResultAsArgFromRef(Category category, int position, RefAlignParam refAlignParam) { return Context.AccessResultAsArgFromRef(category, position, refAlignParam); }

        internal override Result AccessResultAsContextRefFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Context.AccessResultAsContextRefFromRef(category, position, refAlignParam);
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam.Equals(Context.RefAlignParam));
            return DumpPrintFromRef(category);
        }

        private Result DumpPrintFromRef(Category category)
        {
            var refAlignParam = Context.RefAlignParam;
            var result = new List<Result>();
            for(var i = 0; i < StatementList.Count; i++)
            {
                var accessResult = AccessResultAsArgFromRef(category | Category.Type, i, refAlignParam);
                result.Add(accessResult.Type.DumpPrint(category).UseWithArg(accessResult));
            }
            return Result.ConcatPrintResult(category, result);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            return dest.CreateResult(category,
                                     () => CodeBase.CreateArg(Size.Zero), () => Context.ConstructorRefs());
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionFeature);
            return false;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<ISearchPath<IFeature, Ref>>;
            if(searchVisitorChild != null)
                searchVisitorChild.InternalResult = Context.Container.SearchFromRefToStruct(searchVisitorChild.Defineable).CheckedConvert(this);
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }
    }
}