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
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.Context
{
    sealed class UndefinedSymbolIssue : SyntaxIssue

    {
        [EnableDump]
        readonly ContextBase _context;
        readonly ExpressionSyntax _syntax;

        [EnableDump]
        TokenData Token { get { return _syntax.Token; } }

        internal UndefinedSymbolIssue(ContextBase context, ExpressionSyntax syntax)
            : base(syntax, IssueId.UndefinedSymbol)
        {
            _context = context;
            _syntax = syntax;
            StopByObjectId(266);
        }

        internal override string LogDump
        {
            get
            {
                var result = base.LogDump;
                var probes = Probes.Where(p => p.HasImplementations).ToArray();
                if(probes.Length == 0)
                    return result;
                result += ("\n" + probes.Select(x => x.LogDump).Stringify("\n")).Indent();
                return result;
            }
        }

        [DisableDump]
        Probe[] Probes { get { return _syntax.Probes(_context); } }

        internal static IssueType Type(ContextBase context, ExpressionSyntax syntax) { return new IssueType(new UndefinedSymbolIssue(context, syntax), context.RootContext); }
    }
}