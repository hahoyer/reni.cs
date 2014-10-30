using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class SyntaxError : DumpableObject, IType<Syntax>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        Syntax IType<Syntax>.Create(Syntax left, SourcePart part, Syntax right)
        {
            return new CompileSyntaxError(part, _issueId);
        }
        string IType<Syntax>.PrioTableName { get { return PrioTable.Error; } }
        ISubParser<Syntax> IType<Syntax>.Next { get { return null; } }
    }
}