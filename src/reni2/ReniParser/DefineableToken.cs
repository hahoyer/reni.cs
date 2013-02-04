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
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DefineableToken : ReniObject, IIconKeyProvider
    {
        readonly TokenData _data;
        readonly Defineable _tokenClass;

        internal DefineableToken(Defineable tokenClass, TokenData tokenData)
        {
            _data = tokenData;
            _tokenClass = tokenClass;
        }

        public TokenData Data { get { return _data; } }

        [Node]
        internal Defineable TokenClass { get { return _tokenClass; } }

        [DisableDump]
        public string IconKey { get { return "Symbol"; } }

        protected override string GetNodeDump() { return Data.Name.Quote(); }
    }
}