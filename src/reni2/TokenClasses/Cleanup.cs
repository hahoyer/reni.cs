using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Cleanup : TokenClass, IValueProvider
    {
        public const string TokenId = "~~~";
        public override string Id => TokenId;

        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            var statements = left?.GetStatements();
            if(statements == null)
                statements = new Statement[0];
            var cleanup = right?.Value;
            return CompoundSyntax.Create(statements, cleanup);
        }
    }
}