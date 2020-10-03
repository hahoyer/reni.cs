using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Struct;
using Reni.Validation;
using ReniUI.Classification;
using ReniUI.Formatting;
using ReniUI.Helper;

namespace ReniUI
{
    public sealed class CompilerBrowser : DumpableObject
    {
        class CacheContainer
        {
            internal Formatting.BinaryTreeSyntax FomattingBinaryTreeSyntax;
            internal Helper.BinaryTreeSyntax HelperBinaryTreeSyntax;
            internal Helper.Value Value;
        }

        readonly CacheContainer Cache = new CacheContainer();

        readonly IDictionary<IFormalCodeItem, int> CodeToFunctionIndexCache =
            new Dictionary<IFormalCodeItem, int>();

        readonly ValueCache<Compiler> ParentCache;

        internal CompilerBrowser(Func<Compiler> parent) => ParentCache = new ValueCache<Compiler>(parent);

        public Source Source => Compiler.Source;

        public Compiler Compiler => ParentCache.Value;

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
            => Cache.FomattingBinaryTreeSyntax ?? (Cache.FomattingBinaryTreeSyntax = new Formatting.BinaryTreeSyntax(Compiler.BinaryTree));

        internal Helper.BinaryTreeSyntax BinaryTreeSyntax => Cache.HelperBinaryTreeSyntax ?? (Cache.HelperBinaryTreeSyntax = GetHelperSyntax());
        internal Helper.Value Value => Cache.Value?? (Cache.Value= GetValue());


        public static CompilerBrowser FromText
            (string text, CompilerParameters parameters, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, parameters, sourceIdentifier));

        public static CompilerBrowser FromText(string text, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, null, sourceIdentifier));

        public static CompilerBrowser FromFile(string fileName, CompilerParameters parameters = null)
            => new CompilerBrowser(() => Compiler.FromFile(fileName, parameters));

        public Token LocatePosition(int offset)
            => Token.LocateByPosition(Value, offset);

        public Token LocatePosition(SourcePosition current)
        {
            Tracer.Assert(current.Source == Source);
            return LocatePosition(current.Position);
        }

        public IEnumerable<SourcePart> FindAllBelongings(Token open) 
            => open.FindAllBelongings(this);

        public string FlatFormat(bool areEmptyLinesPossible)
            => FormattingBinaryTreeSyntax.FlatFormat(areEmptyLinesPossible);

        internal IEnumerable<Reni.Parser.Value> FindPosition(int offset)
        {
            var enumerable = LocatePosition(offset)
                .Syntax
                .ParentChainIncludingThis
                .Select(item => item.Target)
                .ToArray();

            var compileSyntaxs = enumerable
                .Where(item => item?.ResultCache.Any() ?? false);
            return compileSyntaxs;
        }

        internal FunctionType Function(int index)
            => Compiler.Root.Function(index);


        internal object FindFunction(IFormalCodeItem codeBase)
        {
            var result = FindFunctionIndex(codeBase);
            return result == null
                ? (object)Compiler.CodeContainer.Main.Data
                : Function(result.Value);
        }

        internal int? FindFunctionIndex(IFormalCodeItem codeBase)
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

        internal IEnumerable<Reni.TokenClasses.BinaryTree> FindAllBelongings(Helper.BinaryTreeSyntax binaryTreeSyntax)
            => Compiler.BinaryTree.Belongings(binaryTreeSyntax.Target);

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

            ;
        }

        Helper.Value GetValue()
        {
            try
            {
                return new Value(Compiler.Value);
            }
            catch(Exception e)
            {
                $"Syntax: Unexpected {e} \nText:\n{Source.Data}".Log();
                throw;
            }

            ;
        }

        Helper.Value  LocateValueByPosition(int offset)
        {
            var token = Token.LocateByPosition(Value, offset);
            if(token.IsComment || token.IsLineComment)
                return null;

            var tokenSyntax = token.Syntax;
            var position = tokenSyntax.Token.Characters.Position;
            return position <= 0? null : tokenSyntax.LocateByPosition(position - 1);
        }
    }
}