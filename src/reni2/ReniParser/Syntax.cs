using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    abstract class Syntax : DumpableObject
    {
        protected Syntax() { }

        protected Syntax(int objectId)
            : base(objectId) {}

        [DisableDump]
        internal virtual CompileSyntax ContainerStatementToCompileSyntax => ToCompiledSyntax;

        [DisableDump]
        internal abstract CompileSyntax ToCompiledSyntax { get; }

        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield break;
        }
        internal virtual IEnumerable<string> GetDeclarations() { yield break; }

        internal Syntax CreateThenSyntax(CompileSyntax condition)
            => new CondSyntax(condition, ToCompiledSyntax);

        internal virtual Syntax CreateElseSyntax(CompileSyntax elseSyntax)
        {
            NotImplementedMethod(elseSyntax);
            return null;
        }

        internal virtual Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        static bool _isInDump;

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
        internal virtual CompoundSyntax ToContainer => ToListSyntax.ToContainer;
        [DisableDump]
        internal virtual bool IsMutableSyntax => false;
        [DisableDump]
        internal virtual bool IsConverterSyntax => false;
        [DisableDump]
        internal virtual bool IsIdentifier => false;
        [DisableDump]
        internal virtual bool IsText => false;
        [DisableDump]
        internal virtual bool IsKeyword => false;
        [DisableDump]
        internal virtual bool IsNumber => false;
        [DisableDump]
        internal virtual bool IsError => false;
        [DisableDump]
        internal virtual bool IsBraceLike => false;

        internal virtual Syntax Error
            (IssueId issue, SourcePart token, Syntax right = null)
        {
            if(right == null)
            {
                var e = new Validation.SyntaxError(issue, token);
                return new ProxySyntax(e, this);
            }
            NotImplementedMethod(issue, token, right);
            return null;
        }

        internal virtual Syntax SuffixedBy(Definable definable, SourcePart token)
            => new ExpressionSyntax(ToCompiledSyntax, definable, null, token);

        [DisableDump]
        internal virtual IEnumerable<Issue> Issues
            => Parts.SelectMany(item => item.DirectIssues).ToArray();

        [DisableDump]
        internal virtual IEnumerable<Issue> DirectIssues { get { yield break; } }


        internal virtual Syntax Match(int level, SourcePart token)
            => IssueId.ExtraRightBracket.Syntax(token, this);

        [DisableDump]
        internal IEnumerable<Syntax> Parts => DirectChildren
            .Where(item => item != null)
            .SelectMany(item => item.Parts)
            .plus(this);

        [DisableDump]
        protected virtual IEnumerable<Syntax> DirectChildren { get { yield break; } }

        internal ListSyntax ToListSyntax => new ListSyntax(null, ToList(null));
    }
}