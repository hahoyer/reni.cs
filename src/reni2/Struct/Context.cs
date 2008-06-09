using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    /// ContextAtPosition for structure
    /// </summary>
    internal sealed class Context : Reni.Context.Child
    {
        private readonly Container _container;
        private readonly DictionaryEx<int, TypeBase> _type = new DictionaryEx<int, TypeBase>();
        private CodeBase _contextRefCode;
        private IContextFeature[] _contextFeaturesCache;

        /// <summary>
        /// Initializes a new instance of the StructContainer class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="container">The struc.</param>
        /// created 12.12.2006 21:29
        internal Context(ContextBase parent, Container container)
            : base(parent)
        {
            _container = container;
            Tracer.ConditionalBreak(Parent is Context && ((Context) Parent).Container == Container, "");
        }

        [Node, DumpData(false)]
        private IContextFeature[] ContextFeatures
        {
            get
            {
                if(_contextFeaturesCache == null)
                    _contextFeaturesCache = CreateContextFeaturesCache();
                return _contextFeaturesCache;
            }
        }

        /// <summary>
        /// Returns the type of an element
        /// </summary>
        /// <param name="position">Index of the element</param>
        internal TypeBase VisitType(int position)
        {
            return _container.VisitType(Parent, position);
        }

        /// <summary>
        /// Gets the struct.
        /// </summary>
        /// <value>The struct.</value>
        /// created 16.12.2006 23:49
        internal Container Container { get { return _container; } }

        /// <summary>
        /// Creates the type of the struct.
        /// </summary>
        /// <param name="currentCompilePosition">The current compile position.</param>
        /// <returns></returns>
        /// created 02.01.2007 15:34
        public Type CreateStructType(int currentCompilePosition)
        {
            return (Type)
                _type.Find
                    (
                    currentCompilePosition,
                    () => new Type(Parent, _container, currentCompilePosition)
                    );
        }

        [DumpData(false)]
        internal CodeBase ContextRefCode
        {
            get
            {
                if(_contextRefCode == null)
                    _contextRefCode = CreateContextRefCode();
                return _contextRefCode;
            }
        }

        private CodeBase CreateContextRefCode()
        {
            return CodeBase.CreateContextRef(this);
        }

        internal override CodeBase CreateRefForStruct(Type type)
        {
            if(Parent != type.Context)
                return null;
            if(Container != type.Container)
                return null;
            if(Container.List.Count > type.CurrentCompilePosition)
                return null;
            return Container.CreateRef(Parent);
        }

        internal IContextFeature AtFeatureObject()
        {
            return Container.AtFeatureObject(Parent);
        }

        /// <summary>
        /// Visits the element at a given position.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// created 15.12.2006 09:22
        internal Result VisitElementFromContextRef(Category category, int index)
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

        private TypeBase VisitElementType(int index)
        {
            return _container[index].VisitType(this);
        }

        internal Size Offset(int index)
        {
            return _container.Offset(Parent, index);
        }

        internal Size BackOffset(int index)
        {
            return _container.BackOffset(Parent, index);
        }

        [DumpData(false)]
        private Context ContextRefForCode { get { return Parent.CreateStructContext(_container); } }

        public IContextFeature[] CreateContextFeaturesCache()
        {
            return Container.CreateContextFeaturesCache(Parent);
        }
        
        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            var containerResult = _container.Search(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature, this);
            if (result.IsSuccessFull)
                return result;
            return Parent.Search(defineable).AlternativeTrial(result);
        }

        public IContextFeature CreateMemberAccess(int i)
        {
            return ContextFeatures[i];
        }
    }
}