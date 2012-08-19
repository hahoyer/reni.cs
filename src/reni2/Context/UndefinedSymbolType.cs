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
using Reni.Code;
using Reni.Parser;
using Reni.Validation;

namespace Reni.Context
{
    sealed class UndefinedSymbolIssue : IssueBase
    {
        internal interface IIssueSource 
        {
            string FileErrorPosition(string tag);
        }

        [EnableDump]
        readonly ContextBase _context;
        [EnableDump]
        readonly IIssueSource _source;

        internal UndefinedSymbolIssue(ContextBase context, IIssueSource source)
        {
            _context = context;
            _source = source;
        }
        internal static IssueType CreateType(ContextBase target, IIssueSource source) { return new IssueType(new UndefinedSymbolIssue(target, source), target.RootContext); }
        internal override string LogDump
        {
            get
            {
                var result = _source.FileErrorPosition(Tag);
                return result;
            }
        }
        static string Tag { get { return "UNDEF_SYMBOL"; } }
    }
}