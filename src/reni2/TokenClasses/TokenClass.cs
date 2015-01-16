using System.Collections.Generic;
using System.Linq;
using System;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base class for compiler tokens
    /// </summary>
    abstract class TokenClass : TokenClass<Syntax>, IOperator<Syntax>
    {
        protected override sealed Syntax Create(Syntax left, SourcePart token, Syntax right) => this.Operation(left, token, right);

        internal Syntax CreateForVisit(Syntax left, SourcePart token, Syntax right) => this.Operation(left, token, right);

        Syntax IOperator<Syntax>.Terminal(SourcePart token) => Terminal(token);
        Syntax IOperator<Syntax>.Prefix(SourcePart token, Syntax right) => Prefix(token, right);
        Syntax IOperator<Syntax>.Suffix(Syntax left, SourcePart token) => Suffix(left, token);
        Syntax IOperator<Syntax>.Infix(Syntax left, SourcePart token, Syntax right) => Infix(left, token, right);

        protected virtual Syntax Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual Syntax Prefix(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Syntax Suffix(Syntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}