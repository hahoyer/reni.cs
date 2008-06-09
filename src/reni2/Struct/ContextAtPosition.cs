using System.Collections.Generic;
using HWClassLibrary.Debug;
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
    internal sealed class ContextAtPosition : Reni.Context.Child
    {
        [DumpData(false)]
        private readonly Context _parent;
        [DumpData(true)]
        private readonly int _currentCompilePosition;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="currentCompilePosition">The currentCompilePosition.</param>
        public ContextAtPosition(Context parent, int currentCompilePosition)
            : base(parent)
        {
            _parent = parent;
            _currentCompilePosition = currentCompilePosition;
        }

        private TypeBase VisitBodyType()
        {
            return _parent.VisitType(_currentCompilePosition);
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

    }
}
