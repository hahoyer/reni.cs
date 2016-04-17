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
        internal virtual Result<Value> ContainerStatementToCompileSyntax
            => ToCompiledSyntax;

        [DisableDump]
        internal abstract Result<Value> ToCompiledSyntax { get; }

        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield break;
        }

        internal virtual IEnumerable<string> GetDeclarations() { yield break; }

        internal Result<OldSyntax> CreateThenSyntax(Value condition)
        {
            var syntax = ToCompiledSyntax;
            return new Result<OldSyntax>(new CondSyntax(condition, syntax.Target), syntax.Issues);
        }

        internal virtual OldSyntax CreateElseSyntax(Value elseSyntax)
        {
            NotImplementedMethod(elseSyntax);
            return null;
        }

        internal Result<OldSyntax> CreateElseSyntax(Result<Value> right)
            => Extension.Issues(CreateElseSyntax(right.Target), right.Issues);

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
        internal virtual bool IsMutableSyntax => false;
        [DisableDump]
        internal virtual bool IsConverterSyntax => false;
        [DisableDump]
        internal virtual bool IsMixInSyntax => false;

        internal virtual Result<OldSyntax> Match(int level, SourcePart token)
            => new Result<OldSyntax>(this, IssueId.ExtraRightBracket.CreateIssue(token));

        [DisableDump]
        protected virtual IEnumerable<OldSyntax> DirectChildren { get { yield break; } }

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

        internal virtual IEnumerable<OldSyntax> GetMixinsFromBody()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual Result<OldSyntax> RightSyntax(OldSyntax right, SourcePart token)
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

        internal virtual Result<OldSyntax> InfixOfMatched(SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

    }

    abstract class NonCompileSyntax : OldSyntax
    {
        internal override SourcePart Token { get; }
        protected NonCompileSyntax(SourcePart token) { Token = token; }
        [DisableDump]
        internal override Result<Value> ToCompiledSyntax
            =>
                new Result<Value>
                    (new EmptyList(Token), IssueId.InvalidExpression.CreateIssue(Token));
    }
}