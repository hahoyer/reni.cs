#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using Reni.Basics;
using Reni.Feature;
using Reni.ReniParser;

namespace Reni
{
    abstract class RootSearchVisitor<TFeature> : SearchVisitor<TFeature>
        where TFeature : class, ISearchPath
    {
        readonly DictionaryEx<System.Type, Probe> _probes;
        readonly ISearchTarget _target;
        protected TFeature Result { get; private set; }

        internal RootSearchVisitor(ISearchTarget target, ExpressionSyntax syntax)
            : base(syntax)
        {
            _probes = new DictionaryEx<System.Type, Probe>(Probe.Create);
            _target = target;
            ConversionFunctions = new IConversionFunction[0];
        }

        internal override sealed IConversionFunction[] ConversionFunctions { get; set; }
        internal override bool IsSuccessFull { get { return Result != null; } }

        internal override TFeature InternalResult
        {
            set
            {
                if(Result != null && !Result.IsEqual(value))
                {
                    var rrr = ((ISimpleFeature)Result).Result(Category.All);
                    var vvv = ((ISimpleFeature)value).Result(Category.All);
                }

                Tracer.Assert(Result == null || Result.IsEqual(value), ()=> "Result= "+Result +"\nvalue= "+value);

                Result = value;
            }
        }

        internal override ISearchTarget Target { get { return _target; } }
        internal override DictionaryEx<System.Type, Probe> Probes { get { return _probes; } }
    }
}