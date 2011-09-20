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
    static class AccessManager
    {
        internal interface IAccessObject
        {
            Result DumpPrintOperationResult(AccessType accessType, Category category);
            Result ValueReferenceViaFieldReference(Category category, AccessType accessType);
            bool IsDataLess(AccessType accessType);
        }

        sealed class FunctionAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.DumpPrintOperationResult(AccessType accessType, Category category) { return accessType.DumpPrintFunctionResult(category); }
            Result IAccessObject.ValueReferenceViaFieldReference(Category category, AccessType accessType) { return accessType.ValueReferenceViaFieldReferenceFunction(category); }
            bool IAccessObject.IsDataLess(AccessType accessType) { return true; }
        }

        sealed class FieldAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.DumpPrintOperationResult(AccessType accessType, Category category) { return accessType.DumpPrintFieldResult(category); }
            Result IAccessObject.ValueReferenceViaFieldReference(Category category, AccessType accessType) { return accessType.ValueReferenceViaFieldReferenceField(category); }
            bool IAccessObject.IsDataLess(AccessType accessType) { return accessType.ValueType.IsDataLess; }
        }

        sealed class ProcedureCallAccessObject : ReniObject, IAccessObject
        {
            Result IAccessObject.DumpPrintOperationResult(AccessType accessType, Category category) { return accessType.DumpPrintProcedureCallResult(category); }
            bool IAccessObject.IsDataLess(AccessType accessType) { return true; }

            Result IAccessObject.ValueReferenceViaFieldReference(Category category, AccessType accessType)
            {
                NotImplementedMethod(category, accessType);
                return null;
            }
        }

        internal static readonly IAccessObject Function = new FunctionAccessObject();
        internal static readonly IAccessObject Field = new FieldAccessObject();
        internal static readonly IAccessObject ProcedureCall = new ProcedureCallAccessObject();
    }
}