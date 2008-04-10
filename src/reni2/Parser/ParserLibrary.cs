using System;
using System.Reflection;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    /// <summary>
    /// Class to scan and create tokens
    /// </summary>
    public class ParserLibrary : ReniObject
    {
        private readonly char[] _charType = new char[256];

        /// <summary>
        /// ctor
        /// </summary>
        public ParserLibrary()
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

        private bool IsDigit(char Char)
        {
            return _charType[Char] == '0';
        }

        private bool IsAlpha(char Char)
        {
            return _charType[Char] == 'a';
        }

        private bool IsSymbol(char Char)
        {
            return _charType[Char] == '*';
        }

        private bool IsWhiteSpace(char Char)
        {
            return _charType[Char] == ' ';
        }

        private bool IsAlphaNum(char Char)
        {
            return IsAlpha(Char) || IsDigit(Char);
        }

        /// <summary>
        /// Scans source for begin of next token, advances and returns the new token. 
        /// </summary>
        /// <param name="sp">Source position, is advanced during create token</param>
        /// <returns>the next token</returns>
        internal Token CreateToken(SourcePosn sp)
        {
            for(;;)
            {
                JumpWhiteSpace(sp);

                if(sp.IsEnd())
                    return new Token(sp, 0, RPar.Frame);
                if(IsDigit(sp.Current))
                    return CreateNumberToken(sp);
                if(IsAlpha(sp.Current))
                    return CreateNameToken(sp);
                if(IsSymbol(sp.Current))
                    return CreateSymbolToken(sp);

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

        private Token CreateNameToken(SourcePosn sp)
        {
            var i = 1;
            while(IsAlphaNum(sp[i]))
                i++;
            return Token.CreateToken(false, sp, i);
        }

        private Token CreateSymbolToken(SourcePosn sp)
        {
            var i = 1;
            while(IsSymbol(sp[i]))
                i++;
            return Token.CreateToken(true, sp, i);
        }

        private static Token JumpComment(SourcePosn sp)
        {
            var i = 0;
            if(sp.SubString(0, 3) == "#(#")
            {
                i += 6;
                while(sp[i - 1] != '\0' && sp.SubString(i - 3, 3) != "#)#")
                    i++;
                if(sp[i - 1] == '\0')
                    return new Token(sp, i - 1, _syntaxErrorEOFComment);
            }
            else
            {
                i += 2;
                while(sp[i - 1] != '\0' && sp[i - 1] != '\n')
                    i++;
            }
            sp.Incr(i);
            return null;
        }

        private static readonly SyntaxError _syntaxErrorEOFComment = new SyntaxError("unexpected end of file in comment");

        private static Token CreateStringToken(SourcePosn sp)
        {
            NotImplementedFunction(sp);
            return null;
        }

        private void SetCharType(char Type, string Chars)
        {
            for(var i = 0; i < Chars.Length; i++)
                _charType[Chars[i]] = Type;
        }
    }
}