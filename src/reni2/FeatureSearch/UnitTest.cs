#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace Reni.FeatureSearch
{
    [TestFixture]
    public sealed class UnitTest : DependantAttribute
    {
        [Test]
        public void Simple()
        {
            var type = new Type();
            var symbol = new Symbol();

            var feature = type.Search(symbol);
            Tracer.Assert(feature != null);
            Tracer.Assert(false);
        }

        abstract class SymbolBase : DumpableObject
        {
            internal virtual Feature GetFeature(TypeBase type) { return null; }
        }

        sealed class Symbol : SymbolBase
        {
            internal override Feature GetFeature(TypeBase type) { return type.GetFeature<Symbol>(); }
        }

        abstract class TypeBase : DumpableObject
        {
            public Feature Search(SymbolBase symbol) { return symbol.GetFeature(this); }

            public Feature GetFeature<T>()
            {
                var provider = this as ISymbolProvider<T>;
                return provider == null ? null : provider.Feature;
            }
        }

        interface ISymbolProvider<T>
        {
            Feature Feature { get; }
        }

        sealed class Type : TypeBase, ISymbolProvider<Symbol>
        {
            public Feature Feature { get { return new Feature(); } }
        }

        class Feature : DumpableObject
        {}
    }
}