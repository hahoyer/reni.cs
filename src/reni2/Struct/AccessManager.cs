//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Type;

namespace Reni.Struct
{
    internal static class AccessManager
    {
        internal interface IAccessObject
        {
            Result DumpPrintOperationResult(AccessType accessType, Category category);
            TypeBase ValueType(AccessType accessType);
            Result ValueReferenceViaFieldReference(Category category, AccessType accessType);
        }

        private sealed class FunctionAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.DumpPrintOperationResult(AccessType accessType, Category category) { return accessType.DumpPrintFunctionResult(category); }
            TypeBase IAccessObject.ValueType(AccessType accessType) { return accessType.ValueTypeFunction; }

            Result IAccessObject.ValueReferenceViaFieldReference(Category category, AccessType accessType)
            {
                NotImplementedMethod(category, accessType);
                return null;
            }
        }

        private sealed class FieldAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.DumpPrintOperationResult(AccessType accessType, Category category) { return accessType.DumpPrintFieldResult(category); }
            TypeBase IAccessObject.ValueType(AccessType accessType) { return accessType.ValueTypeField; }
            Result IAccessObject.ValueReferenceViaFieldReference(Category category, AccessType accessType) { return accessType.ValueReferenceViaFieldReferenceField(category); }
        }

        private sealed class ProcedureCallAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.DumpPrintOperationResult(AccessType accessType, Category category) { return accessType.DumpPrintProcedureCallResult(category); }

            TypeBase IAccessObject.ValueType(AccessType accessType)
            {
                NotImplementedMethod(accessType);
                return null;
            }

            Result IAccessObject.ValueReferenceViaFieldReference(Category category, AccessType accessType)
            {
                NotImplementedMethod(category, accessType);
                return null;
            }
        }

        private sealed class PropertyAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.DumpPrintOperationResult(AccessType accessType, Category category)
            {
                NotImplementedMethod(accessType, category);
                return null;
            }

            TypeBase IAccessObject.ValueType(AccessType accessType) { return accessType.ValueTypeProperty; }
            Result IAccessObject.ValueReferenceViaFieldReference(Category category, AccessType accessType) { return accessType.ValueReferenceViaFieldReferenceProperty(category); }
        }

        internal static readonly IAccessObject Function = new FunctionAccessObject();
        internal static readonly IAccessObject Field = new FieldAccessObject();
        internal static readonly IAccessObject ProcedureCall = new ProcedureCallAccessObject();
        internal static readonly IAccessObject Property = new PropertyAccessObject();
    }
}