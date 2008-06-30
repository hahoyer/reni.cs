using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Type : TypeBase
    {
        private readonly Container _container;
        private readonly ContextBase _context;
        private readonly int _currentCompilePosition;
        private IFeature[] _featuresCache;

        public Type(ContextBase context, Container struc, int currentCompilePosition)
        {
            _context = context;
            _container = struc;
            _currentCompilePosition = currentCompilePosition;
            _atFeatureObject = new AtFeature(Container, Context);
        }

        [Node]
        internal int CurrentCompilePosition { get { return _currentCompilePosition; } }
        [Node]
        internal Container Container { get { return _container; } }
        [Node]
        internal ContextBase Context { get { return _context; } }
        internal override Size Size { get { return _container.PartialSize(_context, _currentCompilePosition); } }
        [DumpData(false)]
        internal override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# (" + _container.DumpPrintText(_context) + ")"; } }
        internal override bool IsPending { get { return _container.IsPendingType(_context); } }

        internal override Result MoveHandler(Category category)
        {
            return _container.MoveHandler(category, _context, _currentCompilePosition);
        }

        internal override bool HasConverterTo(TypeBase dest)
        {
            return _container.HasConverterTo(_context, dest);
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            var voidDest = dest as Reni.Type.Void;
            if(voidDest != null)
                return _container.IsConvertableToVoid(_context);
            return base.IsConvertableToVirt(dest, conversionFeature);
        }

        internal override CodeBase CreateRefCodeForContext(ContextBase context)
        {
            return context.CreateRefForStruct(this);
        }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return _container.ConvertTo(category, _context, dest);
        }

        internal override Result DestructorHandler(Category category)
        {
            return _container.DestructorHandler
                (
                _context,
                category,
                CreateRef(_context.RefAlignParam).CreateArgResult(category),
                _currentCompilePosition
                );
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            return _container.DumpPrintFromRef(category, _context, refAlignParam);
        }

        public Result AccessFromArg(Category category, int position)
        {
            var result = _container.VisitElementFromContextRef(_context, category, position);
            var containerContext = _context.CreateStructContext(_container);
            var argsRef = CodeBase
                .CreateArg(_context.RefAlignParam.RefSize)
                .CreateRefPlus(_context.RefAlignParam, Size);
            return result.ReplaceRelativeContextRef(containerContext, argsRef);
        }

        public override string DumpShort()
        {
            return "context." + _context.ObjectId + "(container." + _container.ObjectId + ", context." + ObjectId + ")";
        }

        internal override SearchResult<IFeature> Search(Defineable defineable)
        {
            var containerResult = _container.Search(defineable);
            return containerResult.SearchResultDescriptor.Convert(containerResult.Feature, this);
        }

        [DumpData(false)]
        private IFeature[] Features
        {
            get
            {
                if (_featuresCache == null)
                    _featuresCache = CreateFeaturesCache();
                return _featuresCache;
            }
        }

        public IFeature CreateMemberAccess(int index)
        {
            return Features[index];
        }
        private IFeature[] CreateFeaturesCache()
        {
            var result = new List<StructContainerFeature>();
            for (var i = 0; i < Container.List.Count; i++)
                result.Add(new StructContainerFeature(this, i));
            return result.ToArray();
        }

        sealed private class StructContainerFeature : IFeature
        {
            [DumpData(true)]
            private readonly Type _type;
            [DumpData(true)]
            private readonly int _index;

            public StructContainerFeature(Type type, int index)
            {
                _type = type;
                _index = index;
            }

            public Result Result(ContextBase callContext, Category category, ICompileSyntax args, Ref callObject)
            {
                return _type
                    .Container
                    .VisitAccessApply(_type.Context, _index, callContext, category, args);
            }

            public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
            {
                return _type
                    .Container
                    .AccessApplyResult(_type.Context, _index, callContext, category, @object, args);
            }
        }

        private readonly AtFeature _atFeatureObject;

        public IFeature AtFeatureObject()
        {
            return _atFeatureObject;
        }
    }
}