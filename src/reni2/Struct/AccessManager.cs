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
            Result AccessFromThisReference(Category category, AccessPoint accessPoint, int position);
            AccessFeature ToProperty(ContainerContextObject containerContextObject, int position, bool isProperty);
        }

        private sealed class FunctionAccessObject: ReniObject, IAccessObject{
            Result IAccessObject.AccessFromThisReference(Category category, AccessPoint accessPoint, int position)
            {
                NotImplementedMethod(category,accessPoint,position);
                return null;
            }

            AccessFeature IAccessObject.ToProperty(ContainerContextObject containerContextObject, int position, bool isProperty)
            {
                NotImplementedMethod(containerContextObject, position, isProperty);
                return null;
            }
        }

        private sealed class FieldAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.AccessFromThisReference(Category category, AccessPoint accessPoint, int position)
            {
                return accessPoint.ContainerContextObject.AccessFromThisReference(category, accessPoint.Position, position);
            }

            AccessFeature IAccessObject.ToProperty(ContainerContextObject containerContextObject, int position, bool isProperty)
            {
                NotImplementedMethod(containerContextObject, position, isProperty);
                return null;
            }
        }

        private sealed class ProcedureCallAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.AccessFromThisReference(Category category, AccessPoint accessPoint, int position)
            {
                NotImplementedMethod(category, accessPoint, position);
                return null;
            }
            AccessFeature IAccessObject.ToProperty(ContainerContextObject containerContextObject, int position, bool isProperty)
            {
                NotImplementedMethod(containerContextObject, position, isProperty);
                return null;
            }
        }

        private sealed class PropertyAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.AccessFromThisReference(Category category, AccessPoint accessPoint, int position)
            {
                NotImplementedMethod(category, accessPoint, position);
                return null;
            }
            AccessFeature IAccessObject.ToProperty(ContainerContextObject containerContextObject, int position, bool isProperty)
            {
                NotImplementedMethod(containerContextObject, position, isProperty);
                return null;
            }
        }

        internal static readonly IAccessObject Function = new FunctionAccessObject();
        internal static readonly IAccessObject Field = new FieldAccessObject();
        internal static readonly IAccessObject ProcedureCall = new ProcedureCallAccessObject();
        internal static readonly IAccessObject Property = new PropertyAccessObject();

    }
}