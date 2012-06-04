using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal abstract class CodeBaseException : Exception
    {
        private readonly IContextReference _container;
        protected CodeBaseException(IContextReference container) { _container = container; }
        public override string Message { get { return _container.ToString(); } }
    }

    internal sealed class UnexpectedContextReference : CodeBaseException
    {
        internal UnexpectedContextReference(IContextReference container)
            : base(container) { }

    }
}