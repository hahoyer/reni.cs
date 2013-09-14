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
using Reni.Code;
using Reni.Context;
using Reni.ReniParser;

namespace Reni.Validation
{
    public abstract class IssueBase : ReniObject
    {
        internal static readonly IEnumerable<IssueBase> Empty = new IssueBase[0];
        readonly FunctionCache<ExpressionSyntax, ConsequentialError> _consequentialError;
        internal readonly IssueId IssueId;

        internal IssueBase(IssueId issueId)
        {
            IssueId = issueId;
            _consequentialError = new FunctionCache<ExpressionSyntax, ConsequentialError>(syntax => new ConsequentialError(syntax,this));
        }

        internal abstract string LogDump { get; }
        internal ConsequentialError ConsequentialError(ExpressionSyntax syntax) { return _consequentialError[syntax]; }
        protected string Tag { get { return IssueId.Tag; } }
        internal IssueType Type(Root rootContext) { return new IssueType(this, rootContext); }
        internal virtual CodeBase Code { get { return CodeBase.Issue(this); } }
    }
}