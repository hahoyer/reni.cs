//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Syntax
{
    [Serializable]
    internal abstract class CompileSyntax : ReniParser.ParsedSyntax, ICompileSyntax
    {
        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly DictionaryEx<ContextBase, object> ResultCache = new DictionaryEx<ContextBase, object>();

        internal CompileSyntax(TokenData token)
            : base(token) { }

        internal CompileSyntax(TokenData token, int objectId)
            : base(token, objectId) { }

        string ICompileSyntax.DumpShort() { return DumpShort(); }
        string ICompileSyntax.FilePosition() { return FilePosition(); }
        void ICompileSyntax.AddToCacheForDebug(ContextBase context, object cacheItem) { ResultCache.Add(context, cacheItem); }
        TokenData ICompileSyntax.FirstToken { get { return GetFirstToken(); } }
        TokenData ICompileSyntax.LastToken { get { return GetLastToken(); } }
        bool ICompileSyntax.IsLambda { get { return GetIsLambda(); } }
        string ICompileSyntax.DumpPrintText { get { return DumpPrintText; } }
        [DisableDump]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        protected virtual bool GetIsLambda() { return false; }

        Result ICompileSyntax.Result(ContextBase context, Category category) { return Result(context, category); }

        internal virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal override ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData token, TokenData rightToken) { return this; }

        internal override ICompileSyntax ToCompiledSyntax() { return this; }
        internal override ReniParser.ParsedSyntax CreateSyntaxOrDeclaration(Defineable tokenClass, TokenData token, ReniParser.ParsedSyntax right) { return new ExpressionSyntax(tokenClass, this, token, right.ToCompiledSyntaxOrNull()); }
    }
}