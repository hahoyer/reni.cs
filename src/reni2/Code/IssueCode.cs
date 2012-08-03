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
using Reni.Basics;

namespace Reni.Code
{
    sealed class IssueCode : CodeBase
    {
        static int _nextObjectId;
        readonly IssueBase[] _issue;

        internal IssueCode(IssueBase issue)
            : base(_nextObjectId++) { _issue = new[] {issue}; }
        protected override Size GetSize() { return Size.Zero; }
        internal override CodeBase Add(FiberItem subsequentElement) { throw new NotImplementedException(); }
        internal override IEnumerable<IssueBase> Issues { get { return _issue; } }
        internal override void Visit(IVisitor visitor) { }
    }
}