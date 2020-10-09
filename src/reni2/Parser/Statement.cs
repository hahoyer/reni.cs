using System;
using hw.DebugFormatter;

namespace Reni.Parser
{
    [Obsolete("", true)]
    sealed class Statement : DumpableObject
    {
        [EnableDump]
        internal ValueSyntax Body { get; }

        [EnableDump]
        readonly Declarer Declarer;


        Statement(Declarer declarer, ValueSyntax body)
        {
            Body = body;
            Declarer = declarer?? new Declarer(null,null,null);
            
        }


        internal static Result<Statement[]> CreateStatements(Result<ValueSyntax> value)
            => Create(value).Convert(x => new[] {x});

        static Result<Statement> Create(Result<ValueSyntax> value)
            => value.Convert(x => new Statement(null, x));

        internal static Result<Statement> Create(Declarer declarer, Result<ValueSyntax> body)
            => body.Convert(x => new Statement(declarer, x));
    }

    interface IDefaultScopeProvider
    {
        bool MeansPublic { get; }
    }
}