// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
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
    sealed class AutoCallType : FunctionalBodyType
    {
        internal AutoCallType(FunctionalBody parent)
            : base(parent) { }


        Result ValueResult(Category category) { return FunctionalFeature.ApplyResult(category, Void.Result(category.Typed), RefAlignParam); }

        internal override void Search(SearchVisitor searchVisitor)
        {
            base.Search(searchVisitor);
            searchVisitor.Search(ValueType, new ConversionFunction(this));
        }

        sealed class ConversionFunction : Reni.ConversionFunction
        {
            readonly AutoCallType _parent;
            public ConversionFunction(AutoCallType parent) { _parent = parent; }
            [DisableDump]
            internal override TypeBase ArgType { get { return _parent; } }
            internal override Result Result(Category category) { return _parent.ValueResult(category); }
        }

        [DisableDump]
        TypeBase ValueType { get { return ValueResult(Category.Type).Type; } }
        [DisableDump]
        protected override TypeBase ArgsType { get { return Void; } }
        [DisableDump]
        protected override string Tag { get { return "/!\\"; } }
    }
}