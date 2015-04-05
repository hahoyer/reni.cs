using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Parser;

namespace Reni.TokenClasses
{
    sealed class DefaultFormat : DumpableObject, IFormattingConfiguration
    {
        public static readonly IFormattingConfiguration Instance = new DefaultFormat();

        string IFormattingConfiguration.Parenthesis(IToken left, string content, IToken right)
        {
            if(left.PrecededWith.Any() || right.PrecededWith.Any())
                NotImplementedMethod(left, content, right);
            return left.Id + content + right.Id;
        }

        string IFormattingConfiguration.List
            (string id, IEnumerable<WhiteSpaceToken[]> whiteSpaces, IEnumerable<string> content)
        {
            if(whiteSpaces.Any(item => item.Any()))
                NotImplementedMethod(id, whiteSpaces.ToArray(), content.ToArray());
            return content.Stringify(id + " ");
        }

        string IFormattingConfiguration.Terminal(IToken token)
        {
            if(token.PrecededWith.Any())
                NotImplementedMethod(token);
            return token.Id;
        }
    }
}