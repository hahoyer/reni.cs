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

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    ///     Handle argument replaces
    /// </summary>
    abstract class ReplaceArg : Base
    {
        static int _nextObjectId;
        readonly Result _actualArg;

        internal ReplaceArg(Result actualArg)
            : base(_nextObjectId++)
        {
            Tracer.Assert(actualArg != null, () => "actualArg != null");
            Tracer.Assert(actualArg.HasCode, () => "actualArg.HasCode");
            //Tracer.Assert(actualArg.HasType, () => "actualArg.HasType");
            _actualArg = actualArg;
        }

        [DisableDump]
        protected Result ActualArg { get { return _actualArg; } }

        protected abstract CodeBase Actual { get; }

        internal override CodeBase Arg(Arg visitedObject)
        {
            if(ActualArg.Type == visitedObject.Type)
                return Actual;
            var conversion = ActualArg
                .Type
                .ObviousExactConversion(Category.Code.Typed, visitedObject.Type);
            if(conversion != null)
                return conversion
                    .ReplaceArg(ActualArg)
                    .Code;
            throw new SizeException(Actual, visitedObject);
        }

        [Dump("Dump")]
        internal sealed class SizeException : Exception
        {
            readonly CodeBase _actual;
            readonly Arg _visitedObject;

            public SizeException(CodeBase actual, Arg visitedObject)
            {
                _actual = actual;
                _visitedObject = visitedObject;
            }

            [UsedImplicitly]
            public string Dump()
            {
                var data = "\nVisitedObject="
                           + Tracer.Dump(_visitedObject)
                           + "\nActual="
                           + Tracer.Dump(_actual);

                return "SizeException\n{"
                       + data.Indent()
                       + "\n}";
            }
        }
    }
}