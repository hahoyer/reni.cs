using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class MainInformation : DumpableObject
    {
        readonly SmartFormat _parent;
        readonly SourceSyntax _body;
        readonly WhiteSpaceToken[] _whiteSpaces;
        readonly EndToken _end;

        MainInformation
            (SmartFormat parent, SourceSyntax body, WhiteSpaceToken[] whiteSpaces, EndToken end)
        {
            _parent = parent;
            _body = body;
            _whiteSpaces = whiteSpaces;
            _end = end;
        }

        internal static MainInformation Create(SourceSyntax target, SmartFormat parent)
        {
            var end = target.TokenClass as EndToken;
            if(end == null)
                return null;

            Tracer.Assert(target.Right == null);
            return new MainInformation(parent, target.Left, target.Token.PrecededWith, end);
        }

        public IEnumerable<Item> LineModeReformat(int indentLevel)
            => _parent
                .IndentedReformatLineMode(_body, _whiteSpaces, _end, indentLevel);
    }
}