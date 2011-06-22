using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Type;

namespace Reni.Struct
{
    internal static class AccessManager
    {
        internal interface IAccessObject
        {
            Result AccessViaContextReference(Category category, Structure accessPoint, int position);
        }

        private sealed class FunctionAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.AccessViaContextReference(Category category, Structure accessPoint, int position) { return accessPoint.FunctionAccess(category, position); }
        }

        private sealed class FieldAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.AccessViaContextReference(Category category, Structure accessPoint, int position) { return accessPoint.FieldAccess(category, position); }
        }

        private sealed class ProcedureCallAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.AccessViaContextReference(Category category, Structure accessPoint, int position) { return TypeBase.VoidResult(category); }
        }

        private sealed class PropertyAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.AccessViaContextReference(Category category, Structure accessPoint, int position)
            {
                NotImplementedMethod(category, accessPoint, position);
                return null;
            }
        }

        internal static readonly IAccessObject Function = new FunctionAccessObject();
        internal static readonly IAccessObject Field = new FieldAccessObject();
        internal static readonly IAccessObject ProcedureCall = new ProcedureCallAccessObject();
        internal static readonly IAccessObject Property = new PropertyAccessObject();
    }
}