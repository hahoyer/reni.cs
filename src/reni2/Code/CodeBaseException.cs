using System;

namespace Reni.Code
{
    internal abstract class CodeBaseException : Exception
    {
        private readonly Container _container;
        private readonly CodeBase _visitedObject;

        protected CodeBaseException(Container container,
                                    CodeBase visitedObject)
        {
            _container = container;
            _visitedObject = visitedObject;
        }

        internal Container Container
        {
            get { return _container; }
        }

        internal CodeBase VisitedObject
        {
            get { return _visitedObject; }
        }
    }

    internal sealed class UnexpectedInternalRefInContainer : CodeBaseException
    {
        public UnexpectedInternalRefInContainer(Container container, CodeBase visitedObject)
            : base(container, visitedObject) { }
    }

    internal sealed class UnexpectedContextRefInContainer : CodeBaseException
    {
        internal UnexpectedContextRefInContainer(Container container, CodeBase visitedObject)
            : base(container, visitedObject) { }
    }

}