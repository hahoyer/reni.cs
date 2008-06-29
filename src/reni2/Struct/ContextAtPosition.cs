using HWClassLibrary.Debug;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ContextAtPosition : Reni.Context.Child
    {
        [DumpData(false)]
        private readonly Context _parent;
        [DumpData(true)]
        private readonly int _currentCompilePosition;

        internal ContextAtPosition(Context parent, int currentCompilePosition)
            : base(parent)
        {
            _parent = parent;
            _currentCompilePosition = currentCompilePosition;
        }

        private TypeBase VisitBodyType()
        {
            return _parent.VisitType(_currentCompilePosition);
        }

        internal TypeBase CreateRef()
        {
            return VisitBodyType().CreateRef(RefAlignParam);
        }
    }
}