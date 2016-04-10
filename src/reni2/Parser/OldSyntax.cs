using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.DeclarationOptions;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    abstract class OldSyntax : DumpableObject
    {
        static bool _isInDump;
        SourcePart SourcePartCache;

        internal virtual SourcePart Token => null;

        protected OldSyntax() { }

        protected OldSyntax(int objectId)
            : base(objectId) {}

        [DisableDump]
        internal SourcePart SourcePart
        {
            get { return SourcePartCache ?? (SourcePartCache = GetSourcePartForCache()); }
            set { SourcePartCache = value; }
        }

        [DisableDump]
        internal virtual Checked<Value> ContainerStatementToCompileSyntax
            => ToCompiledSyntax;

        [DisableDump]
        internal abstract Checked<Value> ToCompiledSyntax { get; }

        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield break;
        }

        internal virtual IEnumerable<string> GetDeclarations() { yield break; }

        internal Checked<OldSyntax> CreateThenSyntax(Value condition)
        {
            var syntax = ToCompiledSyntax;
            return new Checked<OldSyntax>(new CondSyntax(condition, syntax.Value), syntax.Issues);
        }

        internal virtual OldSyntax CreateElseSyntax(Value elseSyntax)
        {
            NotImplementedMethod(elseSyntax);
            return null;
        }

        internal Checked<OldSyntax> CreateElseSyntax(Checked<Value> right)
            => CreateElseSyntax(right.Value).Issues(right.Issues);

        internal virtual Checked<OldSyntax> CreateDeclarationSyntax(SourcePart token, OldSyntax right)
            => IssueId.IdentifierExpected.Syntax(token, this, right);

        internal Checked<OldSyntax> CreateDeclarationSyntax
            (SourcePart token, EmptyList right, Issue issue)
        {
            var result = CreateDeclarationSyntax(token, right);
            return result.Value.Issues(issue.plus(result.Issues));
        }

        protected sealed override string Dump(bool isRecursion)
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

        internal virtual IEnumerable<OldSyntax> ToList(List type) { yield return this; }
        [DisableDump]
        internal virtual Checked<CompoundSyntax> ToCompound => ToListSyntax.ToCompound;
        [DisableDump]
        internal virtual bool IsMutableSyntax => false;
        [DisableDump]
        internal virtual bool IsConverterSyntax => false;
        [DisableDump]
        internal virtual bool IsMixInSyntax => false;

        internal virtual Checked<OldSyntax> SuffixedBy(Definable definable, SourcePart token)
            => Checked<OldSyntax>
                .From(ExpressionSyntax.OldCreate(this, definable, null, token));

        internal virtual Checked<OldSyntax> Match(int level, SourcePart token)
            => new Checked<OldSyntax>(this, IssueId.ExtraRightBracket.CreateIssue(token));

        [DisableDump]
        protected virtual IEnumerable<OldSyntax> DirectChildren { get { yield break; } }

        internal ListSyntax ToListSyntax => new ListSyntax(null, ToList(null));

        [DisableDump]
        public IEnumerable<OldSyntax> Closure
            => DirectChildren
                .Where(item => item != null)
                .SelectMany(item => item.Closure)
                .plus(this);

        internal Typeoid Typeoid
        {
            get
            {
                NotImplementedMethod();
                return null;

            }
        }

        internal virtual Checked<DeclaratorTags> ExclamationSyntax(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        internal virtual Checked<DeclaratorTags> Combine(DeclaratorTags syntax)
        {
            NotImplementedMethod(syntax);
            return null;
        }

        internal Checked<OldSyntax> Issues(params Issue[] issues) => new Checked<OldSyntax>(this, issues);

        internal virtual IEnumerable<OldSyntax> GetMixinsFromBody()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual Checked<OldSyntax> RightSyntax(OldSyntax right, SourcePart token)
        {
            NotImplementedMethod(right, token);
            return null;
        }

        SourcePart GetSourcePartForCache()
        {
            return DirectChildren
                .Select(item => item?.SourcePart)
                .Where(item => item != null)
                .Aggregate() + Token;
        }

        internal virtual Checked<OldSyntax> InfixOfMatched(SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        internal Checked<OldSyntax> Cleanup(SourcePart token, OldSyntax rawCleanupSection)
        {
            var currentCompound = ToCompound;
            var cleanupSection = rawCleanupSection.ToCompiledSyntax;
            var result = currentCompound
                .SaveValue
                .AddCleanupSection(token, cleanupSection.SaveValue);
            return new Checked<OldSyntax>(result, currentCompound.Issues.plus(cleanupSection.Issues));
        }
    }

    abstract class NonCompileSyntax : OldSyntax
    {
        internal override SourcePart Token { get; }
        protected NonCompileSyntax(SourcePart token) { Token = token; }
        [DisableDump]
        internal override Checked<Value> ToCompiledSyntax
            =>
                new Checked<Value>
                    (new EmptyList(Token), IssueId.InvalidExpression.CreateIssue(Token));
    }
}