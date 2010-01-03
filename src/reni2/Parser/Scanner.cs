using System;
using System.Collections.Generic;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    [Serializable]
    internal class Scanner : ReniObject, IScanner
    {
        private readonly char[] _charType = new char[256];

        /// <summary>
        /// ctor
        /// </summary>
        public Scanner()
        {
            InitCharType();
        }

        private void InitCharType()
        {
            _charType[0] = '?';
            for(var i = 1; i < 256; i++)
                _charType[i] = '*';
            SetCharType(' ', " \t\n\r");
            SetCharType('0', "0123456789");
            SetCharType('a', "qwertzuiopasdfghjklyxcvbnmQWERTZUIOPASDFGHJKLYXCVBNM_");
            SetCharType('?', "#'\"({[)}];,");
        }

        private bool IsDigit(char @char)
        {
            return _charType[@char] == '0';
        }

        private bool IsAlpha(char @char)
        {
            return _charType[@char] == 'a';
        }

        private bool IsSymbol(char @char)
        {
            return _charType[@char] == '*';
        }

        private bool IsWhiteSpace(char @char)
        {
            return _charType[@char] == ' ';
        }

        private bool IsAlphaNum(char @char)
        {
            return IsAlpha(@char) || IsDigit(@char);
        }

        /// <summary>
        /// Scans source for begin of next token, advances and returns the new token.
        /// </summary>
        /// <param name="sp">Source position, is advanced during create token</param>
        /// <param name="tokenFactory">The token factory.</param>
        /// <returns>the next token</returns>
        Token IScanner.CreateToken(SourcePosn sp, ITokenFactory tokenFactory)
        {
            for(;;)
            {
                JumpWhiteSpace(sp);

                if(sp.IsEnd())
                    return new Token(sp, 0, RPar.Frame);
                if(IsDigit(sp.Current))
                    return CreateNumberToken(sp);
                if(IsAlpha(sp.Current))
                    return CreateNameToken(sp, tokenFactory);
                if(IsSymbol(sp.Current))
                    return CreateSymbolToken(sp, tokenFactory);

                switch(sp.Current)
                {
                    case '#':
                        {
                            var error = JumpComment(sp);
                            if(error != null)
                                return error;
                            break;
                        }

                    case '"':
                        return CreateStringToken(sp);
                    case '\'':
                        return CreateStringToken(sp);

                    case '(':
                        return new Token(sp, 1, LPar.Parenthesis);
                    case '[':
                        return new Token(sp, 1, LPar.Bracket);
                    case '{':
                        return new Token(sp, 1, LPar.Brace);

                    case ')':
                        return new Token(sp, 1, RPar.Parenthesis);
                    case ']':
                        return new Token(sp, 1, RPar.Bracket);
                    case '}':
                        return new Token(sp, 1, RPar.Brace);

                    case ';':
                    case ',':
                        return new Token(sp, 1, List.Instance);
                    default:
                        DumpMethodWithBreak("not implemented", sp);
                        throw new NotImplementedException();
                }
            }
        }

        private void JumpWhiteSpace(SourcePosn sp)
        {
            var i = 0;
            while(IsWhiteSpace(sp[i]))
                i++;
            sp.Incr(i);
        }

        private Token CreateNumberToken(SourcePosn sp)
        {
            var i = 1;
            while(IsDigit(sp[i]))
                i++;
            return new Token(sp, i, Number.Instance);
        }

        private Token CreateNameToken(SourcePosn sp, ITokenFactory tokenFactory)
        {
            var i = 1;
            while(IsAlphaNum(sp[i]))
                i++;
            return tokenFactory.CreateToken(sp, i);
        }

        private Token CreateSymbolToken(SourcePosn sp, ITokenFactory tokenFactory)
        {
            var i = 1;
            while(IsSymbol(sp[i]))
                i++;
            return tokenFactory.CreateToken(sp, i);
        }

        private Token JumpComment(SourcePosn sp)
        {
            var closingParenthesis = "?)}]"["({[".IndexOf(sp[1]) + 1];

            if(closingParenthesis == '?')
            {
                JumpSingleLineComment(sp);
                return null;
            }

            int? errorPosition = null;
            var i = 3;
            var endOfComment = closingParenthesis + "#";
            if (IsSymbol(sp[2]))
                endOfComment = sp[2] + endOfComment;
            else if(IsAlpha(sp[2]))
            {
                while (IsAlphaNum(sp[i]))
                    i++;
                endOfComment = sp.SubString(2, i - 2) + endOfComment;
            }
            else
                errorPosition = 2;


            while(sp[i] != '\0' && IsWhiteSpace(sp[i]) && sp.SubString(i+1, endOfComment.Length) != endOfComment)
                i++ ;
            if(sp[i] == '\0')
                return new Token(sp, i, _syntaxErrorEOFComment);

            sp.Incr(i + 1 + endOfComment.Length);
            if (errorPosition == null)
                return null;
            return new Token(sp, i, _syntaxErrorBeginComment);
        }

        private static void JumpSingleLineComment(SourcePosn sp)
        {
            var i = 2;
            while (sp[i - 1] != '\0' && sp[i - 1] != '\n')
                i++;
            sp.Incr(i);
        }

        private static readonly SyntaxError _syntaxErrorEOFComment = new SyntaxError("unexpected end of file in comment");
        private static readonly SyntaxError _syntaxErrorBeginComment = new SyntaxError("invalid opening character for comment");

        private static Token CreateStringToken(SourcePosn sp)
        {
            NotImplementedFunction(sp);
            return null;
        }

        private void SetCharType(char type, IEnumerable<char> chars)
        {
            foreach(var t in chars)
                _charType[t] = type;
        }
    }
}