using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser;
using Base=Reni.Type.Base;

namespace Reni.Struct
{
    /// <summary>
    /// ContextAtPosition for structure
    /// </summary>
    internal sealed class Context : Child
    {
        private readonly Container _container;
        private readonly DictionaryEx<int, Base> _type = new DictionaryEx<int, Base>();
        private Code.Base _contextRefCode;

        /// <summary>
        /// Initializes a new instance of the StructContainer class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="container">The struc.</param>
        /// created 12.12.2006 21:29
        internal Context(Reni.Context.Base parent, Container container)
            : base(parent)
        {
            _container = container;
            Tracer.ConditionalBreak(Parent is Context && ((Context)Parent).Container == Container, "");
        }

        /// <summary>
        /// Returns the type of an element
        /// </summary>
        /// <param name="position">Index of the element</param>
        internal Base VisitType(int position)
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
                       delegate { return new Type(Parent, _container, currentCompilePosition); }
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

        internal override ContextSearchResult SearchDefineable(DefineableToken defineableToken)
        {
            return Container.Search(defineableToken).ToContextSearchResult(CreateStructType(Container.List.Count));
        }

        internal override Code.Base CreateRefForStruct(Type type)
        {
            if (Parent != type.Context)
                return null;
            if (Container != type.Container)
                return null;
            if (Container.List.Count > type.CurrentCompilePosition)
                return null;
            return Container.CreateRef(Parent);
        }
    }
}