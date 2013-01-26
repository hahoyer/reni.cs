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
using Reni.ReniParser;
using Reni.Struct;
using Reni.Type;

namespace Reni.Validation
{
    sealed class IssueType : TypeBase
    {
        [EnableDump]
        readonly IssueBase _issue;
        readonly Root _rootContext;

        public IssueType(IssueBase issue, Root rootContext)
        {
            _issue = issue;
            _rootContext = rootContext;
        }
        internal override void Search(SearchVisitor searchVisitor) { searchVisitor.Search(this); }
        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }
        [DisableDump]
        internal override bool IsDataLess { get { return true; } }

        internal Result IssueResult(Category category) { return Result(category, getCode: Code); }
        IssueType ConsequentialErrorType(ExpressionSyntax syntax) { return _issue.ConsequentialError(syntax).Type(RootContext); }

        CodeBase Code() { return _issue.Code; }
        internal ISearchPath SearchResult(ISearchTarget target, ExpressionSyntax syntax) { return new ImplicitSearchResult(this, target, syntax); }

        internal sealed class ImplicitSearchResult
            : ReniObject
              , ISuffixFeature
              , ISearchPath<ISuffixFeature, FunctionType>, ISimpleFeature

        {
            [EnableDump]
            readonly IssueType _parent;
            [EnableDump]
            readonly ISearchTarget _target;
            readonly ExpressionSyntax _syntax;

            public ImplicitSearchResult(IssueType parent, ISearchTarget target, ExpressionSyntax syntax)
            {
                _parent = parent;
                _target = target;
                _syntax = syntax;
            }

            IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
            IFunctionFeature IFeature.Function { get { return null; } }
            ISimpleFeature IFeature.Simple { get { return this; } }

            ISuffixFeature ISearchPath<ISuffixFeature, FunctionType>.Convert(FunctionType type) { return this; }
            Result ISimpleFeature.Result(Category category)
            {
                return _parent
                    .ConsequentialErrorType(_syntax)
                    .IssueResult(category);
            }
        }
    }
}