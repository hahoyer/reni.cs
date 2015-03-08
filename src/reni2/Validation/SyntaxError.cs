using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.Validation
{
    sealed class SyntaxError : Syntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly SyntaxError _previous;

        public SyntaxError
            (
            IssueId issueId,
            hw.Parser.Token token,
            SyntaxError previous = null,
            SourcePart sourcePart = null)
            : base(token, sourcePart)
        {
            _issueId = issueId;
            _previous = previous;
        }

        internal override Syntax Sourround(SourcePart sourcePart)
            => new SyntaxError(_issueId, Token, _previous, SourcePart + sourcePart);

        internal override bool IsError => true;

        [DisableDump]
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {_previous};
    }
}