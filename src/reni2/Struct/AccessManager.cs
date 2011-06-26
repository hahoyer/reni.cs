using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Struct
{
    internal static class AccessManager
    {
        internal interface IAccessObject
        {}

        private sealed class FunctionAccessObject : ReniObject, IAccessObject
        {}

        private sealed class FieldAccessObject : ReniObject, IAccessObject
        {}

        private sealed class ProcedureCallAccessObject : ReniObject, IAccessObject
        {}

        private sealed class PropertyAccessObject : ReniObject, IAccessObject
        {}

        internal static readonly IAccessObject Function = new FunctionAccessObject();
        internal static readonly IAccessObject Field = new FieldAccessObject();
        internal static readonly IAccessObject ProcedureCall = new ProcedureCallAccessObject();
        internal static readonly IAccessObject Property = new PropertyAccessObject();
    }
}