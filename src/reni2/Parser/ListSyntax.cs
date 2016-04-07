using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ListSyntax : OldSyntax
    {
        public static OldSyntax Create(params OldSyntax[] values)
            => new ListSyntax(null, values.Where(item => item != null));

        public ListSyntax(List type, IEnumerable<OldSyntax> data)
        {
            Type = type;
            Data = data.ToArray();
            StopByObjectIds();
        }

        [EnableDump]
        List Type { get; }
        [EnableDump]
        OldSyntax[] Data { get; }

        [DisableDump]
        internal override Checked<Value> ToCompiledSyntax
        {
            get
            {
                var result = ToCompound;
                return new Checked<Value>(result.Value, result.Issues);
            }
        }

        [DisableDump]
        internal override Checked<CompoundSyntax> ToCompound
            => new CompoundSyntax(Data);

        internal override IEnumerable<OldSyntax> ToList(List type)
        {
            if(Type == null || type == null || Type == type)
                return Data;

            return new[] {ToCompound.SaveValue};
        }

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren => Data;
    }
}