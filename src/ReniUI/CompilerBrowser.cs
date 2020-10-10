using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.Classification;
using ReniUI.Formatting;

namespace ReniUI
{
    public sealed class CompilerBrowser : DumpableObject
    {
        class CacheContainer
        {
            internal Formatting.BinaryTreeSyntax FormattingBinaryTreeSyntax;
            internal Helper.BinaryTreeSyntax HelperBinaryTreeSyntax;
            internal Helper.Syntax Syntax;
        }

        readonly CacheContainer Cache = new CacheContainer();

        readonly IDictionary<IFormalCodeItem, int> CodeToFunctionIndexCache =
            new Dictionary<IFormalCodeItem, int>();

        readonly ValueCache<Compiler> ParentCache;

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

        internal Formatting.BinaryTreeSyntax FormattingBinaryTreeSyntax
            => Cache.FormattingBinaryTreeSyntax ??
               (Cache.FormattingBinaryTreeSyntax = new Formatting.BinaryTreeSyntax(Compiler.BinaryTree));

        internal Helper.BinaryTreeSyntax BinaryTreeSyntax
            => Cache.HelperBinaryTreeSyntax ?? (Cache.HelperBinaryTreeSyntax = GetHelperSyntax());

        internal Helper.Syntax Syntax => Cache.Syntax ?? (Cache.Syntax = GetValue());


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
            Tracer.Assert(current.Source == Source);
            return LocatePosition(current.Position);
        }

        public IEnumerable<SourcePart> FindAllBelongings(Token open)
            => open.FindAllBelongings(this);

        public string FlatFormat(bool areEmptyLinesPossible)
            => FormattingBinaryTreeSyntax.FlatFormat(areEmptyLinesPossible);

        internal IEnumerable<ValueSyntax> FindPosition(int offset)
            => LocatePosition(offset)
                .Syntax
                .ParentChainIncludingThis
                .Select(item => item.Target)
                .OfType<ValueSyntax>()
                .Where(item => item.ResultCache.Any());

        internal FunctionType Function(int index)
            => Compiler.Root.Function(index);


        internal object FindFunction(IFormalCodeItem codeBase)
        {
            var result = FindFunctionIndex(codeBase);
            return result == null
                ? (object)Compiler.CodeContainer.Main.Data
                : Function(result.Value);
        }

        int? FindFunctionIndex(IFormalCodeItem codeBase)
        {
            if(CodeToFunctionIndexCache.TryGetValue(codeBase, out var result))
                return result;

            var results = Compiler
                .Root
                .FunctionCount
                .Select()
                .Where(item => !CodeToFunctionIndexCache.Values.Contains(item));

            foreach(var index in results)
            {
                var codeItems = Function(index).CodeItems;
                foreach(var item in codeItems)
                    CodeToFunctionIndexCache.Add(item, index);

                if(codeItems.Contains(codeBase))
                    return index;
            }

            return null;
        }


        internal IEnumerable<Syntax> FindAllBelongings(Helper.Syntax syntax)
        {
            if(!(syntax.Target.Target.TokenClass is IBelongingsMatcher matcher))
                return null;

            var sourceSyntaxList = Reni.Helper.Extension.BackChain(Compiler.Syntax, syntax.Target).ToArray();

            var root = sourceSyntaxList
                           .Skip(1)
                           .TakeWhile(item => matcher.IsBelongingTo(item.Target.TokenClass))
                           .LastOrDefault() ??
                       syntax.Target;

            return root?.Target.TokenClass is IBelongingsMatcher rootMatcher
                ? root.ItemsAsLongAs(item => rootMatcher.IsBelongingTo(item.Target.TokenClass)).ToArray()
                : null;
        }

        internal string Reformat(IFormatter formatter = null, SourcePart targetPart = null) =>
            (formatter ?? new Formatting.Configuration().Create())
            .GetEditPieces(this, targetPart)
            .Combine(BinaryTreeSyntax.Target.SourcePart);

        internal Helper.BinaryTreeSyntax Locate(SourcePart span)
        {
            var result = BinaryTreeSyntax.Locate(span);
            if(result != null)
                return result;

            Tracer.TraceBreak();
            NotImplementedMethod(span);
            return null;
        }

        internal void Ensure() => Compiler.Execute();

        internal void Execute(DataStack dataStack) => Compiler.ExecuteFromCode(dataStack);

        internal string[] DeclarationOptions(int offset)
        {
            Ensure();
            return LocateValueByPosition(offset)?.DeclarationOptions ?? new string[0];
        }

        internal IEnumerable<Edit> GetEditPieces(SourcePart sourcePart, IFormatter formatter = null)
            => (formatter ?? new Formatting.Configuration().Create())
                .GetEditPieces(this, sourcePart);

        Helper.BinaryTreeSyntax GetHelperSyntax()
        {
            try
            {
                return new Helper.BinaryTreeSyntax(Compiler.BinaryTree);
            }
            catch(Exception e)
            {
                $"Syntax: Unexpected {e} \nText:\n{Source.Data}".Log();
                throw;
            }
        }

        Helper.Syntax GetValue()
        {
            try
            {
                return new Helper.Syntax(Compiler.Syntax);
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

            var tokenSyntax = token.Syntax;
            var position = tokenSyntax.Token.Characters.Position;
            return position <= 0? null : tokenSyntax.LocateByPosition(position - 1);
        }
    }
}