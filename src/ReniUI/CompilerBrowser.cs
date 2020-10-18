using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Helper;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.Validation;
using ReniUI.Classification;
using ReniUI.Formatting;

namespace ReniUI
{
    public sealed class CompilerBrowser : DumpableObject, ValueCache.IContainer
    {
        readonly ValueCache<Compiler> ParentCache;
        readonly PositionDictionary<Helper.Syntax> PositionDictionary = new PositionDictionary<Helper.Syntax>();

        CompilerBrowser(Func<Compiler> parent) => ParentCache = new ValueCache<Compiler>(parent);

        public Source Source => Compiler.Source;

        Compiler Compiler => ParentCache.Value;

        public StringStream Result
        {
            get
            {
                var result = new StringStream();
                Compiler.Parameters.OutStream = result;
                Compiler.Execute();
                return result;
            }
        }

        internal IExecutionContext ExecutionContext => Compiler;

        internal IEnumerable<Issue> Issues => Compiler.Issues;

        internal Helper.Syntax Syntax => this.CachedValue(GetSyntax);

        internal Formatting.Syntax FormattingSyntax
            => this.CachedValue(() => new Formatting.Syntax(Compiler.Syntax));

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();


        public static CompilerBrowser FromText
            (string text, CompilerParameters parameters, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, parameters, sourceIdentifier));

        public static CompilerBrowser FromText(string text, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, null, sourceIdentifier));

        public static CompilerBrowser FromFile(string fileName, CompilerParameters parameters = null)
            => new CompilerBrowser(() => Compiler.FromFile(fileName, parameters));

        public Token LocatePosition(int offset)
            => Token.LocateByPosition(Syntax, offset);

        public Token LocatePosition(SourcePosition current)
        {
            (current.Source == Source).Assert();
            return LocatePosition(current.Position);
        }

        public string FlatFormat(bool areEmptyLinesPossible)
            => FormattingSyntax.FlatFormat(areEmptyLinesPossible);

        internal IEnumerable<ValueSyntax> FindPosition(int offset)
            => LocatePosition(offset)
                .Master
                .Chain(node => node.Parent)
                .Select(item => item.FlatItem)
                .OfType<ValueSyntax>()
                .Where(item => item.ResultCache.Any());

        internal FunctionType Function(int index)
            => Compiler.Root.Function(index);


        internal string Reformat(IFormatter formatter = null, SourcePart targetPart = null) =>
            (formatter ?? new Formatting.Configuration().Create())
            .GetEditPieces(this, targetPart)
            .Combine(Syntax.SourcePart);

        internal Helper.Syntax Locate(SourcePart span)
        {
            var result = Syntax.Locate(span);
            if(result != null)
                return result;

            Tracer.TraceBreak();
            NotImplementedMethod(span);
            return null;
        }

        internal void Ensure() => Compiler.Execute();

        internal void Execute(DataStack dataStack) => Compiler.ExecuteFromCode(dataStack);

        internal IEnumerable<Edit> GetEditPieces(SourcePart sourcePart, IFormatter formatter = null)
            => (formatter ?? new Formatting.Configuration().Create())
                .GetEditPieces(this, sourcePart);

        Helper.Syntax GetSyntax()
        {
            try
            {
                var syntax = new Helper.Syntax(Compiler.Syntax, PositionDictionary);
                PositionDictionary.AssertValid(Compiler.BinaryTree);
                return syntax;
            }
            catch(Exception e)
            {
                $"Syntax: Unexpected {e} \nText:\n{Source.Data}".Log();
                throw;
            }
        }

        Helper.Syntax LocateValueByPosition(int offset)
        {
            var token = Token.LocateByPosition(Syntax, offset);
            if(token.IsComment || token.IsLineComment)
                return null;

            var tokenSyntax = token.Master;
            var position = tokenSyntax.Token.Characters.Position;
            return position <= 0? null : tokenSyntax.LocateByPosition(position - 1);
        }

        internal string[] DeclarationOptions(int offset)
        {
            NotImplementedMethod(offset);
            return default;
        }
    }
}