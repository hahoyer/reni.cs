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
        public ListSyntax(List type, Token token, IEnumerable<Syntax> data)
            : base(token)
        {
            Type = type;
            Data = data.ToArray();
        }

        ListSyntax(ListSyntax other, ParsedSyntax[] parts)
            : base(other,parts)
        {
            Type = other.Type;
            Data = other.Data.ToArray();
        }

        [EnableDump]
        List Type { get; }
        [EnableDump]
        Syntax[] Data { get; }
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => ToContainer;

        [DisableDump]
        internal override CompoundSyntax ToContainer 
            => new CompoundSyntax(Token, Data, Children);

        internal override IEnumerable<Syntax> ToList(List type)
        {
            Tracer.Assert(Type == null || type == null || Type == type, () => Type.Name.Quote() + " != " + type?.Name.Quote());
            return Data;
        }

        internal static ListSyntax Spread(Syntax statement) => new ListSyntax(null, statement.Token, statement.ToList(null));

        protected override IEnumerable<Syntax> SyntaxChildren => Data;

        internal override Syntax Surround(params ParsedSyntax[] parts)
            => new ListSyntax(this, parts);
    }
}