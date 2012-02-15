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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionalBody : ReniObject
    {
        internal readonly Structure Structure;
        internal readonly CompileSyntax Getter;
        internal readonly CompileSyntax Setter;

        internal FunctionalBody(Structure structure, CompileSyntax getter, CompileSyntax setter)
        {
            Structure = structure;
            Getter = getter;
            Setter = setter;
            StopByObjectId(-1);
        }

        internal override string DumpShort()
        {
            return base.DumpShort()
                   + FormatSite(Getter)
                   + "/?\\"
                   + FormatSite(Setter)
                   + "#(#in context."
                   + Structure.ObjectId
                   + "#)#";
        }

        static string FormatSite(CompileSyntax syntax) { return syntax == null ? "" : "(" + syntax.DumpShort() + ")"; }

        internal Result ObtainApplyResult(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -3 && category.HasCode;
            StartMethodDump(trace, category, argsType);
            try
            {
                var argsResult = argsType.ArgResult(category.Typed);
                var result = Structure.Call(category, Getter, argsResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result DumpPrintResult(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}