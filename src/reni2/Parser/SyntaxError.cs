﻿using System;
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
            NotImplementedMethod(left, part, right);
            return null;
        }

        string IType<Syntax>.PrioTableName => PrioTable.Error;
        ISubParser<Syntax> IType<Syntax>.NextParser => null;
        IType<Syntax> IType<Syntax>.NextTypeIfMatched => null;
    }
}