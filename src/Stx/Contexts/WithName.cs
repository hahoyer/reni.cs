using System;
using hw.DebugFormatter;
using hw.Parser;
using Stx.CodeItems;
using Stx.DataTypes;
using Stx.Features;
using Stx.TokenClasses;

namespace Stx.Contexts
{
    sealed class WithName : DumpableObject
    {
        readonly string Name;
        readonly Context Parent;
        WithVariable ContextCache;

        public WithName(Context parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        public Context WithType(DataType dataType)
        {
            if(ContextCache != null)
                Tracer.Assert(dataType == ContextCache.DataType);
            else
                ContextCache = new WithVariable(Parent, Name, dataType);
            return ContextCache;
        }
    }

    sealed class WithVariable : Context
    {
        [EnableDump]
        public readonly DataType DataType;

        [EnableDump]
        public readonly string Name;

        public WithVariable(Context parent, string name, DataType dataType)
            : base(parent)
        {
            Name = name;
            DataType = dataType;
        }

        public override Result Access(UserSymbol name, IToken token, DataType subsctiptionDataType)
        {
            if(!string.Equals(name.Id, Name, StringComparison.InvariantCultureIgnoreCase))
                return base.Access(name, token, subsctiptionDataType);

            if(subsctiptionDataType == null)
                return new Result
                (
                    token.Characters,
                    DataType.Reference,
                    new[]
                    {
                        CodeItem.CreateAccessVariable(Name),
                        CodeItem.CreateSourceHint(token)
                    }
                );
            
            Tracer.Assert(subsctiptionDataType == DataType.Integer);

            var itemType = (DataType as DataTypes.Array)?.ElementType;
            Tracer.Assert(itemType != null);

            return new Result
            (
                token.Characters,
                itemType.Reference,
                new[]
                {
                    CodeItem.CreateArrayAccessVariable(Name,itemType.ByteSize),
                    CodeItem.CreateSourceHint(token)
                }
            );
        }
    }
}