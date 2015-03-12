using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    sealed class ListSyntax : Syntax
    {
        public ListSyntax(List type, IToken token, IEnumerable<Syntax> data)
            : base(token)
        {
            Type = type;
            Data = data.ToArray();
        }

        [EnableDump]
        List Type { get; }
        [EnableDump]
        Syntax[] Data { get; }
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => ToContainer;

        [DisableDump]
        internal override CompoundSyntax ToContainer 
            => new CompoundSyntax(Token, Data);

        internal override IEnumerable<Syntax> ToList(List type)
        {
            Tracer.Assert(Type == null || type == null || Type == type, () => Type.Id.Quote() + " != " + type?.Id.Quote());
            return Data;
        }

        internal static ListSyntax Spread(Syntax statement) => new ListSyntax(null, statement.Token, statement.ToList(null));
        protected override IEnumerable<ReniParser.Syntax> DirectChildren() => Data;
    }
}