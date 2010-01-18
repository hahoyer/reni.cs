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
            var types = StatementList.Select(syntax => Context.Type(syntax)).ToList();
            var dumpPrint = types.Select(typeBase => typeBase.GenericDumpPrint(category)).ToList();
            var result = types
                .Select((type, i) => type.CreateResult(category, () => AccessCodeFromArg(i)))
                .Join(accessResult => accessResult.Type.GenericDumpPrint(category).UseWithArg(accessResult))
                .ToList();
            return Result.ConcatPrintResult(category, result);
        }

        private CodeBase AccessCodeFromArg(int i)
        {
            NotImplementedMethod(i);
            return null;
        }
    }
}