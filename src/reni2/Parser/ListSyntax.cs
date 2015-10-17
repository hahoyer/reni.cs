using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ListSyntax : Syntax
    {
        public static Syntax Create(params Syntax[] values)
            => new ListSyntax(null, values.Where(item => item != null));

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
        internal override Checked<CompileSyntax> ToCompiledSyntax
        {
            get
            {
                var result = ToCompound;
                return new Checked<CompileSyntax>(result.Value, result.Issues);
            }
        }

        [DisableDump]
        internal override Checked<CompoundSyntax> ToCompound
            => new CompoundSyntax(Data);

        internal override IEnumerable<Syntax> ToList(List type)
        {
            if(Type == null || type == null || Type == type)
                return Data;

            return new[] {ToCompound.SaveValue};
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => Data;
    }
}