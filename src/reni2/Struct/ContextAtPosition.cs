using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Base=Reni.Code.Base;

namespace Reni.Struct
{
    /// <summary>
    /// ContextAtPosition for structure
    /// </summary>
    internal sealed class ContextAtPosition : Child
    {
        [DumpData(false)] private readonly Container _container;
        [DumpData(true)] private readonly int _currentCompilePosition;
        private Base _contextRefCodeCache;

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
            Tracer.ConditionalBreak(Parent is Context && ((Context)Parent).Container == Container, "");
        }

        private Reni.Type.TypeBase VisitBodyType()
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
        public Reni.Type.TypeBase CreateRef()
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
            if (_container[index].VisitSize(this).IsZero)
            {
                Result result = Reni.Type.TypeBase.CreateVoidResult(category);
                if (category.HasType) result.Type = VisitElementType(index);
                return result;
            }
            else
            {
                Result result = new Result();
                if (category.HasSize) result.Size = RefAlignParam.RefSize;
                if (category.HasType) result.Type = VisitElementType(index).CreateRef(RefAlignParam);
                if (category.HasCode)
                    result.Code
                        = ContextRefCode
                            .CreateRefPlus(RefAlignParam, BackOffset(index));
                if (category.HasRefs) result.Refs = Refs.Context(ContextRefForCode);
                return result;
            }
        }

        [DumpData(false)]
        private Base ContextRefCode
        {
            get
            {
                if (_contextRefCodeCache == null)
                    _contextRefCodeCache = CreateContextRefCode();
                return _contextRefCodeCache;
            }
        }

        private Base CreateContextRefCode()
        {
            return ContextRefForCode.ContextRefCode;
        }

        [DumpData(false)]
        private Context ContextRefForCode { get { return Parent.CreateStructContext(_container); } }

        [DumpData(false)]
        public Reni.Type.TypeBase IndexType { get { return _container.IndexType; } }

        public Container Container { get { return _container; } }

        internal Size Offset(int index)
        {
            return _container.Offset(Parent, index);
        }

        internal Size BackOffset(int index)
        {
            return _container.BackOffset(Parent, index);
        }

        private Reni.Type.TypeBase VisitElementType(int index)
        {
            return _container[index].VisitType(this);
        }
    }
}