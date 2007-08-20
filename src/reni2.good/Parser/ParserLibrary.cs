using System;
using System.Reflection;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    /// <summary>
    /// Class to scan and create tokens
    /// </summary>
    public class ParserLibrary: ReniObject
    {
        char[] _charType = new char[256];

        void InitCharType()
        {
            _charType[0] = '?';
            for (int i = 1; i < 256; i++) _charType[i] = '*';
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
        /// ctor
        /// </summary>
        public ParserLibrary()
        {
            InitCharType();
        }

        /// <summary>
        /// Scans source for begin of next token, advances and returns the new token. 
        /// </summary>
        /// <param name="sp">Source position, is advanced during create token</param>
        /// <returns>the next token</returns>
        public Token CreateToken(SourcePosn sp)
        {
            for (; ; )
            {
            	JumpWhiteSpace(sp);

                if (sp.IsEnd())
                    return new Token(sp, 0, new RPar(0));
                if (IsDigit(sp.Current))
                    return CreateNumberToken(sp);
                if (IsAlpha(sp.Current))
                    return CreateNameToken(sp);
                if (IsSymbol(sp.Current))
                    return CreateSymbolToken(sp);

                switch (sp.Current)
                {
                    case '#':
                        {
                            Token error = JumpComment(sp);
                            if (error != null)
                                return error;
                            break;
                        }

                    case '"': return CreateStringToken(sp);
                    case '\'': return CreateStringToken(sp);

                    case '(': return new Token(sp, 1, new LPar(3));
                    case '[': return new Token(sp, 1, new LPar(2));
                    case '{': return new Token(sp, 1, new LPar(1));

                    case ')': return new Token(sp, 1, new RPar(3));
                    case ']': return new Token(sp, 1, new RPar(2));
                    case '}': return new Token(sp, 1, new RPar(1));

                    case ';':
                    case ',': return new Token(sp, 1, new List());
					default:
						DumpMethodWithBreak("not implemented",sp);
                		throw new NotImplementedException();
                } ;
            }
        }

        private void JumpWhiteSpace(SourcePosn sp)
        {
            int i = 0;
            while (IsWhiteSpace(sp[i]))
                i++;
            sp.Incr(i);
        }

        private Token CreateNumberToken(SourcePosn sp)
        {
            int i = 1;
            while (IsDigit(sp[i]))
                i++;
            return new Token(sp, i, new Number());
        }

        private Token CreateNameToken(SourcePosn sp)
        {
            int i = 1;
            while (IsAlphaNum(sp[i]))
                i++;
            return CreateToken(false, sp, i);
        }
        private Token CreateSymbolToken(SourcePosn sp)
        {
            int i = 1;
            while (IsSymbol(sp[i]))
                i++;
            return CreateToken(true, sp, i);
        }

        private static Token CreateToken(bool isSymbol, SourcePosn sp, int i)
        {
        	Assembly a = Assembly.GetAssembly(typeof(ParserLibrary));
        	System.Type[] t = a.GetTypes();
            foreach (System.Type tt in t)
            {
                if (IsTokenType(tt.FullName, isSymbol, sp.SubString(0, i)))
                    return new Token(sp, i, (TokenClass.Base)Activator.CreateInstance(tt, new object[0]));
            }
            return new Token(sp, i, new UserSymbol(TokenToTypeNameEnd(isSymbol, sp.SubString(0, i))));
        }
        private static Token JumpComment(SourcePosn sp)
        {
            int i = 0;
            if (sp.SubString(0, 3) == "#(#")
            {
                i += 6;
                while (sp[i - 1] != '\0' && sp.SubString(i - 3, 3) != "#)#")
                    i++;
                if (sp[i - 1] == '\0')
                    return new Token(sp,i-1, new SyntaxError("unexpected end of file in comment"));
            }
            else
            {
                i += 2;
                while (sp[i - 1] != '\0' && sp[i - 1] != '\n')
                    i++;
            }
            sp.Incr(i);
            return null;
        }

        private static Token CreateStringToken(SourcePosn sp)
        {
            throw new NotImplementedException();
        }

        private void SetCharType(char Type, string Chars)
        {
            for (int i = 0; i < Chars.Length; i++)
                _charType[Chars[i]] = Type;
        }
        private static bool IsTokenType(string typeName, bool isSymbol, string token)
        {
            return typeName.EndsWith(TokenToTypeNameEnd(isSymbol,token));
        }

        private static string TokenToTypeNameEnd(bool isSymbol, string token)
        {
            if (isSymbol)
                return ".TokenClass.Symbol." + Symbolize(token);
            else
                return ".TokenClass.Name.T" + token + "T";
        }

        /// <summary>
        /// Symbolizes the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// [created 18.07.2006 23:49]
        public static string Symbolize(string token)
        {
            string name = "";
            for (int i = 0; i < token.Length; i++)
                name += SymbolizeChar(token[i]);
            return name;
        }

        /// <summary>
        /// Symbolizes the char.
        /// </summary>
        /// <param name="Char">The char.</param>
        /// <returns></returns>
        /// [created 18.07.2006 23:38]
        static public string SymbolizeChar(char Char)
        {
            switch (Char)
            {
                case '&': return "And";
                case '\\': return "Backslash";
                case ':': return "Colon";
                case '.': return "Dot";
                case '=': return "Equal";
                case '>': return "Greater";
                case '<': return "Less";
                case '-': return "Minus";
                case '!': return "Not";
                case '|': return "Or";
                case '+': return "Plus";
                case '/': return "Slash";
                case '*': return "Star";
                case '~': return "Tilde";
                default:
                    throw new NotImplementedException("Symbolize("+Char.ToString()+")");
            } ;
        }
    }
}
