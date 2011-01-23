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
        internal IReferenceInCode Container { get { return _container; } }
        public override string Message { get { return Tracer.Dump(this); } }
    }

    internal sealed class UnexpectedContextRefInContainer : CodeBaseException
    {
        internal UnexpectedContextRefInContainer(IReferenceInCode container)
            : base(container) { }
    }
}