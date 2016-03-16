using System.Linq;
using System.Collections.Generic;
using System;
using hw.DebugFormatter;

using Reni.Validation;

namespace Reni.Code
{
    sealed class FunctionContainer : DumpableObject
    {
        [Node]
        internal readonly Container Getter;
        [Node]
        internal readonly Container Setter;

        public FunctionContainer(Container getter, Container setter)
        {
            Getter = getter;
            Setter = setter;
        }

        [DisableDump]
        internal IEnumerable<Issue> Issues
            => Getter.Issues.Union(Setter?.Issues ?? new Issue[0]);
    }
}