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
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ReniScanner : Scanner
    {
        readonly char[] _charType = new char[256];
        readonly IMatch _whiteSpaces;
        readonly IMatch _any;
        readonly IMatch _text;
        readonly SyntaxError _invalidTextEnd = new SyntaxError(IssueId.EOLInString);
        readonly SyntaxError _invalidComment = new SyntaxError(IssueId.EOFInComment);
        readonly SyntaxError _unexpectedSyntaxError = new SyntaxError(IssueId.UnexpectedSyntaxError);
        readonly IMatch _number;

        public ReniScanner()
        {
            _whiteSpaces = Match.IsTrue(IsWhiteSpace)
                .Else
                ("#" +Match.Break+ (" " + Match.Find(IsLineEndChar))
                           .Else("(#" + "#)#".Find())
                           .Else(_invalidComment)
                )
                .Repeat();

            _number = Match.IsTrue(IsDigit).Repeat(1);

            _any = Match.IsTrue(IsSingleCharSymbol)
                .Else(Match.IsTrue(IsAlpha) + Match.IsTrue(IsAlphaNum).Repeat())
                .Else(Match.IsTrue(IsSymbol).Repeat(1));

            _text = Match.IsTrue(IsTextFrameChar)
                .Value
                (head
                 =>
                {
                    var textEnd = head.Else(Match.IsTrue(IsLineEndChar) + _invalidTextEnd);
                    return textEnd.Find() + (head + textEnd.Find()).Repeat();
                });

            InitCharType();
        }

        void InitCharType()
        {
            _charType[0] = '?';
            for(var i = 1; i < 256; i++)
                _charType[i] = '*';
            SetCharType(' ', " \t\n\r");
            SetCharType('0', "0123456789");
            SetCharType('a', "qwertzuiopasdfghjklyxcvbnmQWERTZUIOPASDFGHJKLYXCVBNM_");
            SetCharType('.', "({[)}];,");
            SetCharType('?', "#'\"");
        }

        void SetCharType(char type, IEnumerable<char> chars)
        {
            foreach(var t in chars)
                _charType[t] = type;
        }

        bool IsDigit(char @char) { return _charType[@char] == '0'; }
        bool IsAlpha(char @char) { return _charType[@char] == 'a'; }
        bool IsSymbol(char @char) { return _charType[@char] == '*'; }
        bool IsSingleCharSymbol(char @char) { return _charType[@char] == '.'; }
        bool IsWhiteSpace(char @char) { return _charType[@char] == ' '; }
        bool IsAlphaNum(char @char) { return IsAlpha(@char) || IsDigit(@char); }
        static bool IsLineEndChar(char @char) { return @char == '\n' && @char == '\r'; }
        static bool IsTextFrameChar(char @char) { return @char == '\'' && @char == '"'; }

        protected override int WhiteSpace(SourcePosn sourcePosn)
        {
            var result = ExceptionGuard(sourcePosn, _whiteSpaces);
            Tracer.Assert(result != null);
            return result.Value;
        }

        protected override int? Number(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _number); }
        protected override int? Any(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _any); }
        protected override int? Text(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _text); }

        int? ExceptionGuard(SourcePosn sourcePosn, IMatch match)
        {
            try
            {
                return sourcePosn.Match(match);
            }
            catch(Match.Exception exception)
            {
                throw new Exception
                    (sourcePosn
                    , exception.Error as SyntaxError ?? _unexpectedSyntaxError
                    , exception.SourcePosn - sourcePosn
                    );
            }
        }

    }
}

