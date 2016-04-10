using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Parser
{
    interface IDeclaratorTokenClass
    {
        Checked<Declarator> Get(Syntax left, SourcePart token, Syntax right);
    }

    sealed class Declarator : DumpableObject
    {
        [EnableDump]
        IDeclarationTag[] Tags { get; }

        internal Declarator(IDeclarationTag[] tags) { Tags = tags; }

        internal Checked<Statement> Statement(SourcePart token, Value right)
            => new Statement(Tags, null, token, right);
    }

    interface IDeclaratorTagProvider
    {
        Checked<DeclaratorTags> Get(Syntax left, SourcePart token, Syntax right);
    }

    sealed class DeclaratorTags : NonCompileSyntax
    {
        internal static DeclaratorTags Create(SourcePart token)
            => new DeclaratorTags(new IDeclarationTag[0], token);

        internal readonly IDeclarationTag[] Tags;

        DeclaratorTags(IDeclarationTag[] tags, SourcePart token)
            : base(token) { Tags = tags; }

        internal DeclaratorTags(IDeclarationTag item, SourcePart token)
            : this(item.NullableToArray().ToArray(), token) { }

        internal override Checked<OldSyntax> SuffixedBy(Definable definable, SourcePart token)
            => new DeclaratorSyntax(definable, this);

        internal override Checked<OldSyntax> CreateDeclarationSyntax
            (SourcePart token, OldSyntax right)
            => null;

        [DisableDump]
        internal Checked<Declarator> Declarator => new Declarator(Tags);
    }

    interface IDeclarationTag
    {
    }

    sealed class ExclamationBoxToken : DumpableObject,
        IType<Syntax>,
        ITokenClass,
        IDeclaratorTokenClass
    {
        Syntax Value { get; }

        internal ExclamationBoxToken(Syntax value) { Value = value; }

        Syntax IType<Syntax>.Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return Syntax.CreateSourceSyntax(left, this, token, Value);
        }

        string IType<Syntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";

        Checked<Value> ITokenClass.GetValue(Syntax left, SourcePart token, Syntax right) => null;

        Checked<Declarator> IDeclaratorTokenClass.Get(Syntax left, SourcePart token, Syntax right)
        {
            if(left == null && right != null)
            {
                var exclamationProvider = right.TokenClass as IDeclaratorTagProvider;
                if(exclamationProvider != null)
                {
                    var result = exclamationProvider.Get
                        (right.Left, right.Token.Characters, right.Right);
                    return result.Value.Declarator.With(result.Issues);
                }
            }

            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}