using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser;
using Base=Reni.Type.Base;

namespace Reni.Struct
{
    /// <summary>
    /// Context for structure
    /// </summary>
    public sealed class ContainerContext : Child
    {
        private readonly Container _struct;
        private readonly DictionaryEx<int, Base> _type = new DictionaryEx<int, Base>();
        private Code.Base _contextRefCode;

        /// <summary>
        /// Initializes a new instance of the StructContainer class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="struc">The struc.</param>
        /// created 12.12.2006 21:29
        public ContainerContext(Reni.Context.Base parent, Container struc)
            : base(parent)
        {
            _struct = struc;
        }

        /// <summary>
        /// Returns the type of an element
        /// </summary>
        /// <param name="position">Index of the element</param>
        public Base VisitType(int position)
        {
            return _struct.VisitType(Parent, position);
        }

        /// <summary>
        /// Gets the struct.
        /// </summary>
        /// <value>The struct.</value>
        /// created 16.12.2006 23:49
        public Container Struc { get { return _struct; } }

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
                       delegate { return new Type(Parent, _struct, currentCompilePosition); }
                       );
        }

        [DumpData(false)]
        internal Code.Base ContextRefCode
        {
            get
            {
                if (_contextRefCode == null)
                    _contextRefCode = CreateContextRefCode();
                return _contextRefCode;
            }
        }

        private Code.Base CreateContextRefCode()
        {
            return Code.Base.CreateContextRef(this);
        }

        internal override Code.Base CreateRefForStruct(Type struc)
        {
            if (Parent != struc.Context)
                return null;
            if (Struc != struc.Container)
                return null;
            if (Struc.List.Count > struc.CurrentCompilePosition)
                return null;
            return Struc.CreateRef(Parent);
        }
    }

    /// <summary>
    /// Context for structure
    /// </summary>
    public sealed class Context : Child
    {
        [DumpData(true)] private readonly Container _struct;
        [DumpData(true)] private readonly int _currentCompilePosition;
        private Code.Base _contextRefCode;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="currentCompilePosition">The currentCompilePosition.</param>
        public Context(ContainerContext parent, int currentCompilePosition)
            : base(parent.Parent)
        {
            _struct = parent.Struc;
            _currentCompilePosition = currentCompilePosition;
        }

        private Base VisitBodyType()
        {
            return _struct.VisitType(Parent, _currentCompilePosition);
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

        internal override StructSearchResult SearchDefineable(DefineableToken defineableToken)
        {
            StructAccess structAccess = _struct.SearchDefineable(defineableToken.Name);
            if (structAccess != null)
                return structAccess.ToContextSearchResult(this);

            return Parent.SearchDefineable(defineableToken);
        }

        /// <summary>
        /// Creates the ref.
        /// </summary>
        /// <returns></returns>
        /// created 16.10.2006 22:52
        public Base CreateRef()
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
            if (_struct[index].VisitSize(this).IsZero)
            {
                Result result = Base.CreateVoidResult(category);
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
        private Code.Base ContextRefCode
        {
            get
            {
                if (_contextRefCode == null)
                    _contextRefCode = CreateContextRefCode();
                return _contextRefCode;
            }
        }

        private Code.Base CreateContextRefCode()
        {
            return ContextRefForCode.ContextRefCode;
        }

        [DumpData(false)]
        private ContainerContext ContextRefForCode { get { return Parent.CreateStructContainer(_struct); } }

        [DumpData(false)]
        public Base IndexType { get { return _struct.IndexType; } }

        internal Size Offset(int index)
        {
            return _struct.Offset(Parent, index);
        }

        internal Size BackOffset(int index)
        {
            return _struct.BackOffset(Parent, index);
        }

        private Base VisitElementType(int index)
        {
            return _struct[index].VisitType(this);
        }

        public Result VisitAccessApply(int index, Reni.Context.Base callContext, Category category, Syntax.Base args)
        {
            Category localCategory = category;
            if (args != null)
                localCategory = localCategory | Category.Type;
            Result functionResult = VisitElementFromContextRef(localCategory, index);
            if (args == null)
                return functionResult;

            if (functionResult.IsCodeLess)
                return functionResult.Type.ApplyFunction(callContext, category, args);

            NotImplementedMethod(index, callContext, category, args);
            return null;
        }
    }

    internal sealed class OperationSearchResult : SearchResult
    {
        [DumpData(true)] private readonly Container _container;
        [DumpData(true)] private readonly int _position;
        [DumpData(true)]
        private readonly Reni.Context.Base _context;

        public OperationSearchResult(Type definingType, int position)
            : base(definingType)
        {
            _container = definingType.Container;
            _context = definingType.Context;
            _position = position;
        }

        public override Result VisitApply(Reni.Context.Base callContext, Category category, Syntax.Base args)
        {
            BitsConst indexValue = args.VisitAndEvaluate(callContext, _container.IndexType);
            int index = indexValue.ToInt32();
            return _container.VisitElementFromContextRef(_context, category, index);
        }
    }
}