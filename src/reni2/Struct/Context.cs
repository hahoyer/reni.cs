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
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Struct
{
    sealed class Context : Child
    {
        [Node]
        internal readonly int Position;
        [Node]
        internal readonly Container Container;

        internal Context(ContextBase parent, Container container, int position)
            : base(parent)
        {
            Position = position;
            Container = container;
        }

        void Search(SearchVisitor<IContextFeature> searchVisitor, ContainerContextObject context)
        {
            var feature = Container.SearchFromStructContext(searchVisitor.Defineable);
            if(feature == null)
                return;
            var accessPoint = context.UniqueAccessPoint(Position);
            searchVisitor.InternalResult = feature.ConvertToContextFeature(accessPoint);
            searchVisitor.Add(new ContextFoundItem(accessPoint));
        }

        internal override void Search(SearchVisitor<IContextFeature> searchVisitor)
        {
            Search(searchVisitor, Parent.UniqueContainerContext(Container));
            if(searchVisitor.IsSuccessFull)
                return;
            base.Search(searchVisitor);
        }

        protected override Result ObjectResult(Category category) { return FindRecentStructure.StructReferenceViaContextReference(category); }

        internal override Structure ObtainRecentStructure() { return Parent.UniqueStructure(Container, Position); }
    }

    sealed class ContextFoundItem : ReniObject, IFoundItem
    {
        readonly Structure _accessPoint;
        internal ContextFoundItem(Structure accessPoint) { _accessPoint = accessPoint; }

        Result IFoundItem.Result(Category category, RefAlignParam refAlignParam) { return _accessPoint.StructReferenceViaContextReference(category); }
    }
}