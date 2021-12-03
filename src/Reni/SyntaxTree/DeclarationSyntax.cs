using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    class DeclarationSyntax : Syntax, IStatementSyntax
    {
        [EnableDumpExcept(null)]
        internal readonly DeclarerSyntax Declarer;

        [EnableDumpExcept(null)]
        internal readonly ValueSyntax Value;

        DeclarationSyntax(DeclarerSyntax declarer, ValueSyntax value, Anchor anchor,Issue issue = null)
            : base(anchor,issue)
        {
            Declarer = declarer;
            Value = value;

            Declarer.AssertIsNotNull();
            //Value.AssertIsNotNull();
        }

        [DisableDump]
        DeclarerSyntax IStatementSyntax.Declarer => Declarer;

        SourcePart IStatementSyntax.SourcePart => Anchor.SourcePart;

        ValueSyntax IStatementSyntax.ToValueSyntax(Anchor anchor)
            => CompoundSyntax.Create(T((IStatementSyntax)this), null, anchor);

        [DisableDump]
        ValueSyntax IStatementSyntax.Value => Value;

        IStatementSyntax IStatementSyntax.With(Anchor anchor)
            => anchor == null || !anchor.Items.Any()
                ? this
                : Create(Declarer, Value, anchor.Combine(Anchor));

        [DisableDump]
        protected override int DirectChildCount => Declarer.DirectChildCount + 1;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                { } when index < Declarer.DirectChildCount => Declarer.GetDirectChild(index)
                , { } when index == Declarer.DirectChildCount => Value
                , _ => null
            };

        [DisableDump]
        internal string NameOrNull => Declarer.Name?.Value;

        [DisableDump]
        internal bool IsConverterSyntax => Declarer.IsConverterSyntax;

        [DisableDump]
        internal bool IsMutableSyntax => Declarer.IsMutableSyntax;

        internal static IStatementSyntax Create(DeclarerSyntax declarer, ValueSyntax value, Anchor anchor)
            => new DeclarationSyntax
            (
                declarer,
                value ,
                anchor,
                value == null? IssueId.MissingDeclarationValue.Issue(anchor.Main.SourcePart): null
            );
    }
}