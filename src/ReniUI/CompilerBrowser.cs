using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Helper;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.Classification;
using ReniUI.Formatting;

namespace ReniUI
{
    public sealed class CompilerBrowser : DumpableObject, ValueCache.IContainer
    {
        readonly ValueCache<Compiler> ParentCache;
        readonly PositionDictionary<Helper.Syntax> PositionDictionary = new();

        CompilerBrowser(Func<Compiler> parent) => ParentCache = new ValueCache<Compiler>(parent);

        ValueCache ValueCache.IContainer.Cache { get; } = new();

        public Source Source => Compiler.Source;

        internal Compiler Compiler => ParentCache.Value;

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
        public BinaryTree LeftMost => Compiler.BinaryTree.LeftMost;

        internal IEnumerable<Issue> Issues => Compiler.Issues;

        internal Helper.Syntax Syntax => this.CachedValue(GetSyntax);

        public static CompilerBrowser FromText
            (string text, CompilerParameters parameters, string sourceIdentifier = null)
            => new(() => Compiler.FromText(text, parameters, sourceIdentifier));

        public static CompilerBrowser FromText(string text, string sourceIdentifier = null)
            => new(() => Compiler.FromText(text, null, sourceIdentifier));

        public static CompilerBrowser FromFile(string fileName, CompilerParameters parameters = null)
            => new(() => Compiler.FromFile(fileName, parameters));

        public Item Locate(SourcePosition offset)
        {
            this.CachedItem(GetSyntax).IsValid = true;
            return Item.LocateByPosition(Compiler.BinaryTree, offset);
        }

        public string FlatFormat(bool areEmptyLinesPossible)
            => Syntax.FlatItem.MainAnchor.GetFlatString(areEmptyLinesPossible);

        public Item Locate(int offset) => Locate(Source + offset);

        internal BinaryTree Locate(SourcePart span) 
            => Item.Locate(Compiler.BinaryTree, span);

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

                //var all = syntax.GetNodesFromLeftToRight().ToArray();
                //PositionDictionary.AssertValid(Compiler.BinaryTree);
                //syntax.Dump().FlaggedLine();
                return syntax;
            }
            catch(Exception e)
            {
                $"Syntax: Unexpected {e} \nText:\n{Source.Data}".Log();
                throw;
            }
        }

        internal string[] DeclarationOptions(int offset)
        {
            NotImplementedMethod(offset);
            return default;
        }
    }
}