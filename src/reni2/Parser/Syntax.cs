using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Context;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    abstract class Syntax : DumpableObject
    {
        static bool _isInDump;

        [DisableDump]
        internal SourcePart SourcePart;

        protected Syntax() { }

        protected Syntax(int objectId)
            : base(objectId) { }

        [DisableDump]
        internal virtual Checked<CompileSyntax> ContainerStatementToCompileSyntax
            => ToCompiledSyntax;

        [DisableDump]
        internal abstract Checked<CompileSyntax> ToCompiledSyntax { get; }

        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index) { yield break; }
        internal virtual IEnumerable<string> GetDeclarations() { yield break; }
        internal virtual IEnumerable<Syntax> GetMixins(ContextBase context) { yield break; }

        internal Checked<Syntax> CreateThenSyntax(CompileSyntax condition)
        {
            var syntax = ToCompiledSyntax;
            return new Checked<Syntax>(new CondSyntax(condition, syntax.Value), syntax.Issues);
        }

        internal virtual Syntax CreateElseSyntax(CompileSyntax elseSyntax)
        {
            NotImplementedMethod(elseSyntax);
            return null;
        }

        public Checked<Syntax> CreateElseSyntax(Checked<CompileSyntax> right)
            => CreateElseSyntax(right.Value).Issues(right.Issues);

        internal virtual Checked<Syntax> CreateDeclarationSyntax(SourcePart token, Syntax right)
            => IssueId.IdentifierExpected.Syntax(token, this, right);

        internal Checked<Syntax> CreateDeclarationSyntax
            (SourcePart token, EmptyList right, Issue issue)
        {
            var result = CreateDeclarationSyntax(token, right);
            return result.Value.Issues(issue.plus(result.Issues));
        }

        protected override sealed string Dump(bool isRecursion)
        {
            if(isRecursion)
                return "ObjectId=" + ObjectId;

            var isInContainerDump = CompoundSyntax.IsInContainerDump;
            CompoundSyntax.IsInContainerDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = GetNodeDump();
            if(!ParsedSyntax.IsDetailedDumpRequired)
                return result;
            if(isInContainerDump)
                result += " ObjectId=" + ObjectId;
            else
                result = "\n" + base.Dump(false);
            CompoundSyntax.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        protected override string GetNodeDump() => GetType().PrettyName();

        internal virtual IEnumerable<Syntax> ToList(List type) { yield return this; }
        [DisableDump]
        internal virtual Checked<CompoundSyntax> ToContainer => ToListSyntax.ToContainer;
        [DisableDump]
        internal virtual bool IsMutableSyntax => false;
        [DisableDump]
        internal virtual bool IsConverterSyntax => false;
        [DisableDump]
        internal virtual bool IsMixInSyntax => false;

        internal virtual Checked<Syntax> SuffixedBy(Definable definable, SourcePart token)
        {
            var left = ToCompiledSyntax;
            return new Checked<Syntax>
                (new ExpressionSyntax(left.Value, definable, null, token), left.Issues);
        }

        internal virtual Checked<Syntax> Match(int level, SourcePart token)
            => new Checked<Syntax>(this, IssueId.ExtraRightBracket.CreateIssue(token));

        [DisableDump]
        IEnumerable<Syntax> Parts => DirectChildren
            .Where(item => item != null)
            .SelectMany(item => item.Parts)
            .plus(this);

        [DisableDump]
        protected virtual IEnumerable<Syntax> DirectChildren { get { yield break; } }

        internal ListSyntax ToListSyntax => new ListSyntax(null, ToList(null));

        internal virtual Checked<ExclamationSyntaxList> ExclamationSyntax(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        internal virtual Checked<ExclamationSyntaxList> Combine(ExclamationSyntaxList syntax)
        {
            NotImplementedMethod(syntax);
            return null;
        }

        internal Checked<Syntax> Issues(params Issue[] issues) => new Checked<Syntax>(this, issues);

        internal virtual IEnumerable<Syntax> GetMixinsFromBody()
        {
            NotImplementedMethod();
            return null;
        }
    }

    abstract class NonCompileSyntax : Syntax
    {
        protected readonly SourcePart Token;
        protected NonCompileSyntax(SourcePart token) { Token = token; }
        [DisableDump]
        internal override Checked<CompileSyntax> ToCompiledSyntax
            =>
                new Checked<CompileSyntax>
                    (new EmptyList(Token), IssueId.InvalidExpression.CreateIssue(Token));
    }
}