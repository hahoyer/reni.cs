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
    /// <summary>
    /// ContextAtPosition for structure
    /// </summary>
    internal sealed class ContextAtPosition : Reni.Context.Child
    {
        [DumpData(false)]
        private readonly Container _container;
        [DumpData(true)]
        private readonly int _currentCompilePosition;
        private CodeBase _contextRefCodeCache;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="currentCompilePosition">The currentCompilePosition.</param>
        public ContextAtPosition(Context parent, int currentCompilePosition)
            : base(parent.Parent)
        {
            _container = parent.Container;
            _currentCompilePosition = currentCompilePosition;
            Tracer.ConditionalBreak(Parent is Context && ((Context) Parent).Container == Container, "");
        }

        [DumpData(false)]
        private CodeBase ContextRefCode
        {
            get
            {
                if(_contextRefCodeCache == null)
                    _contextRefCodeCache = CreateContextRefCode();
                return _contextRefCodeCache;
            }
        }
        [DumpData(false)]
        private Context ContextRefForCode { get { return Parent.CreateStructContext(_container); } }

        [DumpData(false)]
        public TypeBase IndexType { get { return _container.IndexType; } }

        public Container Container { get { return _container; } }

        private TypeBase VisitBodyType()
        {
            return _container.VisitType(Parent, _currentCompilePosition);
        }

        /// <summary>
        /// Creates the args ref result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 03.11.2006 22:00
        public override Result CreateArgsRefResult(Category category)
        {
            return Parent.CreateArgsRefResult(category);
        }

        /// <summary>
        /// Creates the ref.
        /// </summary>
        /// <returns></returns>
        /// created 16.10.2006 22:52
        public TypeBase CreateRef()
        {
            return VisitBodyType().CreateRef(RefAlignParam);
        }

        /// <summary>
        /// Visits the element at a given position.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// created 15.12.2006 09:22
        public Result VisitElementFromContextRef(Category category, int index)
        {
            if(_container[index].VisitSize(this).IsZero)
            {
                var result = TypeBase.CreateVoidResult(category);
                if(category.HasType)
                    result.Type = VisitElementType(index);
                return result;
            }
            else
            {
                var result = new Result();
                if(category.HasSize)
                    result.Size = RefAlignParam.RefSize;
                if(category.HasType)
                    result.Type = VisitElementType(index).CreateRef(RefAlignParam);
                if(category.HasCode)
                    result.Code
                        = ContextRefCode
                            .CreateRefPlus(RefAlignParam, BackOffset(index));
                if(category.HasRefs)
                    result.Refs = Refs.Context(ContextRefForCode);
                return result;
            }
        }

        private CodeBase CreateContextRefCode()
        {
            return ContextRefForCode.ContextRefCode;
        }

        internal Size Offset(int index)
        {
            return _container.Offset(Parent, index);
        }

        internal Size BackOffset(int index)
        {
            return _container.BackOffset(Parent, index);
        }

        private TypeBase VisitElementType(int index)
        {
            return _container[index].VisitType(this);
        }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            SearchResult<IStructContainerFeature> containerResult = _container.Search(defineable);
            SearchResult<IContextFeature> result = containerResult.SearchResultDescriptor.
                Convert(containerResult.Feature, this);
            return result;

        }

        public IContextFeature CreateMemberAccess(int i)
        {
            return ContextFeatures[i];
        }

        IContextFeature[] _contextFeaturesCache;
        [Node, DumpData(false)]
        private IContextFeature[] ContextFeatures
        {
            get
            {
                if (_contextFeaturesCache == null)
                    _contextFeaturesCache = CreateContextFeaturesCache();
                return _contextFeaturesCache;
            }
        }

        private IContextFeature[] CreateContextFeaturesCache()
        {
            var result = new List<StructContainerFeature>();
            for (var i = 0; i < Container.List.Count; i++)
                result.Add(new StructContainerFeature(this, i));
            return result.ToArray();
        }

        private class StructContainerFeature : IContextFeature
        {
            [DumpData(true)]
            private readonly ContextAtPosition _contextAtPosition;
            [DumpData(true)]
            private readonly int _i;

            public StructContainerFeature(ContextAtPosition contextAtPosition, int i)
            {
                _contextAtPosition = contextAtPosition;
                _i = i;
            }

            public Result VisitApply(ContextBase contextBase, Category category, SyntaxBase args)
            {
                Tracer.ConditionalBreak(true, Tracer.Dump(this));
                return null;
            }
        }
    }

}