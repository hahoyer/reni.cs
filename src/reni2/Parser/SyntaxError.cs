using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class SyntaxError : DumpableObject, IType<IParsedSyntax>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        IParsedSyntax IType<IParsedSyntax>.Create(IParsedSyntax left, IPart part, IParsedSyntax right, bool isMatch)
        {
            return new CompileSyntaxError((TokenData)part, _issueId);
        }
        string IType<IParsedSyntax>.PrioTableName { get { return PrioTable.Error; } }
        bool IType<IParsedSyntax>.IsEnd { get { return false; } }
    }
}