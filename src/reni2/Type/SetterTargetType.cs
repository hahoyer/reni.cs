#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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
using Reni.Feature;

namespace Reni.Type
{
    abstract class SetterTargetType : TypeBase, ISearchContainerType, IConverter, ISmartReference
    {
        [DisableDump]
        internal readonly ISuffixFeature AssignmentFeature;
        
        protected SetterTargetType()
        {
            AssignmentFeature = new AssignmentFeature(this);
        }

        IConverter ISearchContainerType.Converter { get { return this; } }
        TypeBase ISearchContainerType.TargetType { get { return ValueType; } }
        Result ISmartReference.DereferenceResult(Category category) { return GetterResult(category); }
        Result IConverter.Result(Category category) { return GetterResult(category); }
        TypeBase ISmartReference.TargetType { get { return ValueType; } }

        internal abstract TypeBase ValueType { get; }
        internal abstract Result DestinationResult(Category category);
        internal abstract Result SetterResult(Category category);
        internal abstract Result GetterResult(Category category);

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, ()=>ValueType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }
    }
}