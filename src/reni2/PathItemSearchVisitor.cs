#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Feature;
using Reni.ReniParser;

namespace Reni
{
    sealed class PathItemSearchVisitor<TFeature, TProvider> : SearchVisitor<ISearchPath<TFeature, TProvider>>
        where TFeature : class, ISearchPath
        where TProvider : IFeatureProvider
    {
        internal override DictionaryEx<System.Type, Probe> Probes { get { return _parent.Probes; } }
        [DisableDump]
        readonly SearchVisitor<TFeature> _parent;

        [EnableDump]
        readonly TProvider _provider;

        public PathItemSearchVisitor(SearchVisitor<TFeature> parent, TProvider provider, ExpressionSyntax syntax)
            : base(syntax)
        {
            _parent = parent;
            _provider = provider;
        }

        internal override void Search()
        {
            base.Search();
            _parent.InternalResult = _provider.GetFeature<TFeature>(Target);
        }

        internal override bool IsSuccessFull { get { return _parent.IsSuccessFull; } }
        internal override ISearchTarget Target { get { return _parent.Target; } }

        internal override ISearchPath<TFeature, TProvider> InternalResult
        {
            set
            {
                if(value != null)
                    _parent.InternalResult = value.Convert(_provider);
            }
        }
        internal override IConversionFunction[] ConversionFunctions { get { return _parent.ConversionFunctions; } set { _parent.ConversionFunctions = value; } }
    }
}