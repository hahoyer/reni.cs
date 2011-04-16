using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal abstract class CodeBaseException : Exception
    {
        private readonly IReferenceInCode _container;
        protected CodeBaseException(IReferenceInCode container) { _container = container; }
        public override string Message { get { return _container.ToString(); } }
    }

    internal sealed class UnexpectedContextReference : CodeBaseException
    {
        internal UnexpectedContextReference(IReferenceInCode container)
            : base(container) { }

    }
}