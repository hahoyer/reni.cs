using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    sealed class ListSyntax : Syntax
    {
        [EnableDump]
        readonly List _type;
        [EnableDump]
        readonly Syntax[] _data;

        public ListSyntax(List type, SourcePart token, IEnumerable<Syntax> data)
            : base(token)
        {
            _type = type;
            _data = data.ToArray();
        }

        internal override IEnumerable<Syntax> ToList(List type)
        {
            Tracer.Assert(_type == null || type == null || _type == type, () => type.Name.Quote() + " != " + _type.Name.Quote());
            return _data;
        }

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax { get { return ToContainer; } }

        internal override ContainerSyntax ToContainer { get { return new ContainerSyntax(Token, _data); } }

        internal static ListSyntax Spread(Syntax statement)
        {
            return new ListSyntax(null, statement.Token, statement.ToList(null));
        }
    }
}