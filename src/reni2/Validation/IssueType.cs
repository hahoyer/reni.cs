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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Validation
{
    sealed class IssueType : TypeBase
    {
        readonly IssueBase _issue;
        readonly Root _rootContext;

        internal IssueType(IssueBase issue, Root rootContext)
        {
            _issue = issue;
            _rootContext = rootContext;
        }
        internal override void Search(SearchVisitor searchVisitor) { searchVisitor.Search(this); }
        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }
        [DisableDump]
        internal override bool IsDataLess { get { return true; } }

        internal Result Result(Category category) { return base.Result(category, getCode: Code); }

        CodeBase Code() { return CodeBase.Issue(_issue); }
        public ISearchPath SearchResult(ISearchTarget target) { return new ImplicitSearchResult(this, target); }

        sealed class ImplicitSearchResult : ReniObject, ISuffixFeature
        {
            readonly IssueType _parent;
            readonly ISearchTarget _target;
            public ImplicitSearchResult(IssueType parent, ISearchTarget target)
            {
                _parent = parent;
                _target = target;
            }
            IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
            IFunctionFeature IFeature.Function { get { return null; } }
            ISimpleFeature IFeature.Simple { get { return null; } }
        }
    }
}