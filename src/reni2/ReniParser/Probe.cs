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
using HWClassLibrary.Helper;
using Reni.Feature;

namespace Reni.ReniParser
{
    sealed class Probe : ReniObject
    {
        static readonly DictionaryEx<System.Type, Probe> _probes = new DictionaryEx<System.Type, Probe>(type => new Probe(type));
        readonly System.Type _type;
        readonly SimpleCache<System.Type[]> _subTypesCache;
        readonly SimpleCache<string[]> _instancesCache;

        Probe(System.Type type)
        {
            _type = type;
            _subTypesCache = new SimpleCache<System.Type[]>(() => ParseSearchPath(_type).ToArray());
            _instancesCache = new SimpleCache<string[]>(() => FindInstances(_type).ToArray());
        }

        static IEnumerable<string> FindInstances(System.Type type)
        {
            return MainTokenFactory
                .TokenClasses
                .Where(pair => pair.Value.GetType().Implements(type))
                .Select(pair => pair.Key);

        }

        internal string LogDump
        {
            get
            {
                var result = _subTypesCache.Value.Select(x => x.PrettyName()).Format(" ");
                if(HasImplementations)
                {
                    result += " (implemented as: ";
                    result += _instancesCache.Value.Select(x => x.Quote()).Format(" ");
                    result += ")";
                }
                return result;
            }
        }

        internal bool HasImplementations { get { return _instancesCache.Value.Any(); } }
        internal static Probe Create(System.Type type) { return _probes[type]; }

        static IEnumerable<System.Type> ParseSearchPath(System.Type searchPathType)
        {
            if(!searchPathType.IsGenericType || searchPathType.GetGenericTypeDefinition() != typeof(ISearchPath<,>))
                return new[] {searchPathType};

            var types = searchPathType.GetGenericArguments();
            return ParseSearchPath(types[0]).Union(new[] {types[1]});
        }
    }
}