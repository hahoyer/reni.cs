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
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Syntax;
using Reni.Type;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base clas for compiler tokens
    /// </summary>
    [Serializable]
    abstract class TokenClass : Parser.TokenClass
    {
        protected override sealed IParsedSyntax Syntax
            (IParsedSyntax left
                , TokenData token
                , IParsedSyntax right
            )
        {
            return
                Syntax((ReniParser.ParsedSyntax) left, token, (ReniParser.ParsedSyntax) right);
        }
        protected abstract ReniParser.ParsedSyntax Syntax
            (ReniParser.ParsedSyntax left
                , TokenData token
                , ReniParser.ParsedSyntax right
            );

        internal static Simple Feature(Func<Category, Result> function) { return new Simple(function); }
        internal static Simple<T> Feature<T>(Func<Category, T, Result> function) { return new Simple<T>(function); }

        internal static Feature.Function
            Feature(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return
                new Feature.Function(function);
        }
        internal static Function<T>
            Feature<T>(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return
                new Function<T>(function);
        }

        internal static MetaFunction
            Feature(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function)
        {
            return
                new MetaFunction(function);
        }
    }
}