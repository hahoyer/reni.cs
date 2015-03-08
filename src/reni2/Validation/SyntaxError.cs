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
            Token token,
            SyntaxError previous = null)
            : base(token)
        {
            _issueId = issueId;
            _previous = previous;
        }

        internal override bool IsError => true;

        [DisableDump]
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {_previous};
    }
}