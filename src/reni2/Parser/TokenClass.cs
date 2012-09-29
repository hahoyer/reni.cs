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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;

namespace Reni.Parser
{
    abstract class TokenClass : ReniObject, IIconKeyProvider, ITokenClass
    {
        static int _nextObjectId;
        string _name;

        protected TokenClass()
            : base(_nextObjectId++) { StopByObjectId(-31); }

        [DisableDump]
        string ITokenClass.Name { set { _name = value; } }

        [DisableDump]
        ITokenFactory ITokenClass.NewTokenFactory { get { return NewTokenFactory; } }

        [DisableDump]
        string IIconKeyProvider.IconKey { get { return "Symbol"; } }

        [DisableDump]
        protected virtual ITokenFactory NewTokenFactory { get { return null; } }

        internal override string GetNodeDump() { return base.GetNodeDump() + "(" + Name.Quote() + ")"; }

        [Node]
        [DisableDump]
        internal string Name { get { return _name; } set { _name = value; } }

        string ITokenClass.PrioTableName(string name) { return PrioTableName(name); }
        IParsedSyntax ITokenClass.Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { return Syntax(left, token, right); }

        protected virtual IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual string PrioTableName(string name) { return Name; }

        public override string ToString() { return base.ToString() + " Name=" + _name.Quote(); }
    }
}