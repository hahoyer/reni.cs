#region Copyright (C) 2012

// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    abstract class ReplaceContextRef<TContext> : Base
        where TContext : IContextReference
    {
        static int _nextObjectId;
        protected readonly TContext Context;
        protected readonly Func<CodeBase> Replacement;

        protected ReplaceContextRef(TContext context, Func<CodeBase> replacement)
            : base(_nextObjectId++)
        {
            Context = context;
            Replacement = replacement;
        }

        internal override CodeBase ContextRef(ReferenceCode visitedObject)
        {
            if(visitedObject.Context == (IContextReference) Context)
                return Replacement();
            return null;
        }
    }

    sealed class ReplaceRelativeContextRef<TContext> : ReplaceContextRef<TContext>
        where TContext : IContextReference
    {
        public ReplaceRelativeContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) { }

        protected override Visitor<CodeBase> After(Size size) { return new ReplaceRelativeContextRef<TContext>(Context, () => AfterCode(size)); }

        CodeBase AfterCode(Size size) { return Replacement().ReferencePlus(size); }
    }

    sealed class ReplaceAbsoluteContextRef<TContext> : ReplaceContextRef<TContext>
        where TContext : IContextReference
    {
        public ReplaceAbsoluteContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) { }
    }
}