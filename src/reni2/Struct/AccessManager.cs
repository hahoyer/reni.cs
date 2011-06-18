using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Code;

namespace Reni.Struct
{
    internal static class AccessManager
    {
        internal interface IAccessObject
        {
            Result Access(Category category, AccessPoint accessPoint, int position, bool isFromContextReference);
        }

        private sealed class FunctionAccessObject: ReniObject, IAccessObject{
            Result IAccessObject.Access(Category category, AccessPoint accessPoint, int position, bool isFromContextReference)
            {
                NotImplementedMethod(category, accessPoint, position, isFromContextReference);
                return null;
            }
        }

        private sealed class FieldAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.Access(Category category, AccessPoint accessPoint, int position, bool isFromContextReference)
            {
                return accessPoint.FieldAccess(category, position, isFromContextReference);
            }
        }

        private sealed class ProcedureCallAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.Access(Category category, AccessPoint accessPoint, int position, bool isFromContextReference)
            {
                NotImplementedMethod(category, accessPoint, position, isFromContextReference);
                return null;
            }
        }

        private sealed class PropertyAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.Access(Category category, AccessPoint accessPoint, int position, bool isFromContextReference)
            {
                NotImplementedMethod(category, accessPoint, position, isFromContextReference);
                return null;
            }
        }

        internal static readonly IAccessObject Function = new FunctionAccessObject();
        internal static readonly IAccessObject Field = new FieldAccessObject();
        internal static readonly IAccessObject ProcedureCall = new ProcedureCallAccessObject();
        internal static readonly IAccessObject Property = new PropertyAccessObject();

    }
}