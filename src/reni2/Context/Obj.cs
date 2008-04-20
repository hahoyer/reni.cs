using Reni.Syntax;

namespace Reni.Context
{
    /// <summary>
    /// Class for object context
    /// </summary>
    internal sealed class Obj : Child
    {
        private readonly Type.TypeBase _type;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        /// [created 13.05.2006 22:36]
        public Type.TypeBase Type { get { return _type; } }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="type">The type.</param>
        public Obj(ContextBase parent, Type.TypeBase type) : base(parent)
        {
            _type = type;
        }
    }

    /// <summary>
    /// ContextAtPosition for structure
    /// </summary>
    internal sealed class ObjMemberElem : Child
    {
        private readonly Type.TypeBase _type;
        private readonly MemberElem _memberElem;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type.TypeBase Type { get { return _type; } }
        /// <summary>
        /// Gets the member elem.
        /// </summary>
        /// <value>The member elem.</value>
        /// [created 13.05.2006 22:49]
        public MemberElem MemberElem { get { return _memberElem; } }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="memberElem">The member elem.</param>
        public ObjMemberElem(Obj parent, MemberElem memberElem)
            : base(parent.Parent)
        {
            _type = parent.Type;
            _memberElem = memberElem;
        }

    }
}
