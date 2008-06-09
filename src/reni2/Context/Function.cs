using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    /// Repesents a function call context
    /// </summary>
    internal sealed class Function : Child
    {
        private readonly TypeBase _argsType;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        /// created 03.11.2006 21:54
        public TypeBase ArgsType { get { return _argsType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Function"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="argsType">The type.</param>
        /// created 03.11.2006 21:54
        public Function(ContextBase parent, TypeBase argsType)
            : base(parent)
        {
            _argsType = argsType;
        }

        /// <summary>
        /// Creates the args ref result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 03.11.2006 22:00
        public override Result CreateArgsRefResult(Category category)
        {
            return ArgsType.CreateRef(RefAlignParam).CreateResult
                (
                category,
                () => CodeBase.CreateContextRef(this),
                () => Refs.Context(this)
                )
                ;
        }
    }
}