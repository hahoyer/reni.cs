#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2012 - 2013 Harald Hoyer
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

#endregion

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    abstract class SetterTargetType : TypeBase, IProxyType, IConverter, IReferenceType
    {
        [DisableDump]
        internal readonly ISuffixFeature AssignmentFeature;

        protected SetterTargetType() { AssignmentFeature = new AssignmentFeature(this); }

        IConverter IProxyType.Converter { get { return this; } }
        bool IReferenceType.IsWeak { get { return true; } }
        IConverter IReferenceType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return ValueType; } }
        Result IConverter.Result(Category category) { return GetterResult(category); }

        internal abstract TypeBase ValueType { get; }
        internal abstract Result DestinationResult(Category category);
        internal abstract Result SetterResult(Category category);
        internal abstract Result GetterResult(Category category);
        [DisableDump]
        internal override Root RootContext { get { return ValueType.RootContext; } }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => ValueType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal ResultCache ForceDePointer(Category category)
        {
            var result = GetterResult(category.Typed);
            return result.Type.DePointer(category).Data.ReplaceArg(result);
        }

        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return ValueType.TypeForTypeOperator; } }
        [DisableDump]
        internal override TypeBase ElementTypeForReference { get { return ValueType.ElementTypeForReference; } }
    }
}