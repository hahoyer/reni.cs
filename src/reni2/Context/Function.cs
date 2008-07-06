using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    internal sealed class Function : Child
    {
        private readonly TypeBase _argsType;

        public TypeBase ArgsType { get { return _argsType; } }

        public Function(ContextBase parent, TypeBase argsType)
            : base(parent)
        {
            _argsType = argsType;
        }

        internal override Result CreateArgsRefResult(Category category)
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