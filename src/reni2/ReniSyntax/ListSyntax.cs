using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.ReniParser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    sealed class ListSyntax : Syntax
    {
        public ListSyntax(List type, IEnumerable<Syntax> data)
        {
            Type = type;
            Data = data.ToArray();
            StopByObjectIds();
        }

        [EnableDump]
        List Type { get; }
        [EnableDump]
        Syntax[] Data { get; }
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => ToContainer;

        [DisableDump]
        internal override CompoundSyntax ToContainer
            => new CompoundSyntax(Data);

        internal override IEnumerable<Syntax> ToList(List type)
        {
            Tracer.Assert
                (
                    Type == null || type == null || Type == type,
                    () => Type.Id.Quote() + " != " + type?.Id.Quote());
            return Data;
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => Data;
    }
}