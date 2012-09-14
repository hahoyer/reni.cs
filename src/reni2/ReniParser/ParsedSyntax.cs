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
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Struct;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    [Serializable]
    abstract class ParsedSyntax : ParsedSyntaxBase
    {
        protected ParsedSyntax(TokenData token)
            : base(token) { }

        protected ParsedSyntax(TokenData token, int nextObjectId)
            : base(token, nextObjectId) { }

        internal virtual CompileSyntax ToCompiledSyntax()
        {
            NotImplementedMethod(); //Probably it's a missing right parenthesis
            return null;
        }

        internal virtual ParsedSyntax RightParenthesis(int level, TokenData token)
        {
            NotImplementedMethod(level, token);
            return null;
        }

        internal virtual ParsedSyntax CreateThenSyntax(TokenData token, CompileSyntax condition) { return new ThenSyntax(condition, token, ToCompiledSyntax()); }

        internal virtual ParsedSyntax CreateElseSyntax(TokenData token, CompileSyntax elseSyntax)
        {
            NotImplementedMethod(token, elseSyntax);
            return null;
        }

        internal virtual ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        internal virtual ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken)
        {
            NotImplementedMethod(leftToken, rightToken); //Probably it's a missing right parenthesis
            return null;
        }

        internal virtual ParsedSyntax CreateSyntaxOrDeclaration(Defineable tokenClass, TokenData token, ParsedSyntax right) { return new ExpressionSyntax(tokenClass, ToCompiledSyntax(), token, right.ToCompiledSyntaxOrNull()); }

        static bool _isInDump;

        protected override sealed string Dump(bool isRecursion)
        {
            if(isRecursion)
                return "ObjectId=" + ObjectId;

            var isInContainerDump = Container.IsInContainerDump;
            Container.IsInContainerDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = DumpShort();
            if(!IsDetailedDumpRequired)
                return result;
            if(!isInDump)
                result += FilePosition();
            if(isInContainerDump)
                result += " ObjectId=" + ObjectId;
            else
                result += "\n" + base.Dump(false);
            Container.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }
        internal CompileSyntax MustBeNullError(Func<IssueId> getIssue)
        {
            NotImplementedMethod(getIssue());
            return null;
        }
    }
}