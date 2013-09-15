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
using HWClassLibrary.Helper;

namespace Reni.ReniParser
{
    sealed class Probe : DumpableObject
    {
        static readonly FunctionCache<System.Type, Probe> _probes = new FunctionCache<System.Type, Probe>(type => new Probe(type));
        readonly System.Type _type;
        readonly ValueCache<string[]> _instancesCache;

        Probe(System.Type type)
        {
            _type = type;
            _instancesCache = new ValueCache<string[]>(() => FindInstances(_type).ToArray());
        }

        static IEnumerable<string> FindInstances(System.Type type)
        {
            return MainTokenFactory
                .TokenClasses
                .Where(pair => pair.Value.GetType().Is(type))
                .Select(pair => pair.Key);
        }

        internal bool HasImplementations { get { return _instancesCache.Value.Any(); } }
        internal string LogDump { get { throw new NotImplementedException(); } }

        internal static Probe Create(System.Type type) { return _probes[type]; }
    }
}