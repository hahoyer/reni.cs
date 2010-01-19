using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
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

        [DumpData(false)]
        internal readonly IFeature DumpPrintFeature;

        [DumpData(false)]
        internal readonly IFeature AtFeature;

        private List<TypeBase> _typesCache;

        internal Type(FullContext context)
        {
            AtFeature = new AtToken.Feature(this);
            Context = context;
            DumpPrintFeature = new Feature.DumpPrint.StructFeature(this);
        }

        internal override bool IsValidRefTarget() { return Context.IsValidRefTarget(); }

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

        private IEnumerable<ICompileSyntax> StatementList { get { return Context.StatementList; } }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            return dest.CreateResult
                (
                category,
                () => CodeBase.CreateArg(Size.Zero),
                () => Context.ConstructorRefs()
                );
        }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
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
                searchVisitorChild.InternalResult =
                    Context.Container.SearchFromRefToStruct(searchVisitorChild.Defineable).CheckedConvert(this);
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        internal Result DumpPrint(Category category)
        {
            var offsets = Types
                .Select(typeBase => typeBase.Size)
                .Aggregate(new Size[0], AggregateSizes)
                .ToArray();
            var argCodes = Types
                .Select((type,i)=>AutomaticDereference(type, offsets[i], category))
                .ToArray();
            var dumpPrint = 
                Types
                .Select(type => type.GenericDumpPrint(category))
                .ToArray();
            var replacedResult = dumpPrint
                .Select((r,i)=>r.UseWithArg(argCodes[i]))
                .ToArray();
            var concatPrintResult = Result.ConcatPrintResult(category, replacedResult);
            return concatPrintResult;
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            return type
                .CreateAutomaticRef(Context.RefAlignParam)
                .CreateResult(category, ()=>CreateRefArgCode().CreateRefPlus(Context.RefAlignParam,offset))
                .AutomaticDereference();
        }

        private CodeBase CreateRefArgCode() { return CreateAutomaticRef(Context.RefAlignParam).CreateArgCode(); }

        private static Size[] AggregateSizes(Size[] sizesSoFar, Size nextSize)
        {
            return sizesSoFar.Select(size=>size+nextSize).Union(new[]{Size.Zero}).ToArray();
        }

        private IEnumerable<TypeBase> GetTypes() { return StatementList.Select(syntax => Context.Type(syntax)); }

        private IEnumerable<TypeBase> Types
        {
            get
            {
                if(_typesCache == null)
                    _typesCache = GetTypes().ToList();
                return _typesCache;
            }
        }

    }
}