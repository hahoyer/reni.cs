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
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.CompilationView;
using ReniUI.Formatting;
using ScintillaNET;

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
        public BinaryTree LeftMost=>Compiler.BinaryTree.LeftMost;

        internal IEnumerable<Issue> Issues => Compiler.Issues;

        internal Helper.Syntax Syntax => this.CachedValue(GetSyntax);

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        public static CompilerBrowser FromText
            (string text, CompilerParameters parameters, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, parameters, sourceIdentifier));

        public static CompilerBrowser FromText(string text, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, null, sourceIdentifier));

        public static CompilerBrowser FromFile(string fileName, CompilerParameters parameters = null)
            => new CompilerBrowser(() => Compiler.FromFile(fileName, parameters));

        public Classification.Item LocatePosition(SourcePosition offset)
            => Classification.Item.LocateByPosition(Syntax, offset);

        public string FlatFormat(bool areEmptyLinesPossible)
            => Syntax.FlatItem.MainAnchor.GetFlatFormat(areEmptyLinesPossible);

        public Classification.Item LocatePosition(int offset) => LocatePosition(Source + offset);
        internal IEnumerable<ValueSyntax> FindPosition(int offset) => FindPosition(Source + offset);

        internal IEnumerable<ValueSyntax> FindPosition(SourcePosition offset)
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
            .Combine(Compiler.Source.All);

        internal void Ensure() => Compiler.Execute();

        internal void Execute(DataStack dataStack) => Compiler.ExecuteFromCode(dataStack);

        internal IEnumerable<Edit> GetEditPieces(SourcePart sourcePart, IFormatter formatter = null)
            => (formatter ?? new Formatting.Configuration().Create())
                .GetEditPieces(this, sourcePart);

        Helper.Syntax GetSyntax()
        {
            try
            {
                var compilerSyntax = Compiler.Syntax;
                //compilerSyntax.Dump().FlaggedLine();
                //compilerSyntax.Anchor.Dump().FlaggedLine();

                var syntax = new Helper.Syntax(compilerSyntax, PositionDictionary);

                var all = syntax.GetNodesFromLeftToRight().ToArray();
                PositionDictionary.AssertValid(Compiler.BinaryTree);
                //syntax.Dump().FlaggedLine();
                return syntax;
            }
            catch(Exception e)
            {
                $"Syntax: Unexpected {e} \nText:\n{Source.Data}".Log();
                throw;
            }
        }

        internal Classification.Item LocateIncludingParent(SourcePart span)
        {
            NotImplementedMethod(span);
            return default;
        }

        Classification.Item LocateValueByPosition(SourcePosition offset, bool includingParent)
        {
            var token = Classification.Item.LocateByPosition(Syntax, offset);
            if(token.IsComment || token.IsLineComment)
                return null;

            var start = token.Token.Characters.Start;
            var result = start.Position <= 0? default : token.Master.LocateByPosition(start + -1, includingParent);
            NotImplementedMethod(offset);
            return default;
        }

        internal string[] DeclarationOptions(int offset)
        {
            NotImplementedMethod(offset);
            return default;
        }

        public void SignalStyleNeeded(Scintilla textBox, int position)
        {
            var current = LeftMost;
            while(textBox.GetEndStyled() < position)
            {
                var offset = textBox.GetEndStyled();
                var syntax = LocatePosition(offset);
                var style = TextStyle.From(syntax);
                textBox.StartStyling(syntax.StartPosition);
                textBox.SetStyling(syntax.SourcePart.Length, style);
            }
        }
    }
}