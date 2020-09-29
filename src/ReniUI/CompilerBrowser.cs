using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;
using ReniUI.Classification;
using ReniUI.Formatting;

namespace ReniUI
{
    public sealed class CompilerBrowser : DumpableObject
    {
        class CacheContainer
        {
            internal Syntax FomattingSyntax;
            internal Helper.Syntax HelperSyntax;
        }

        readonly CacheContainer Cache = new CacheContainer();


        public static CompilerBrowser FromText
            (string text, CompilerParameters parameters, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, parameters, sourceIdentifier));

        public static CompilerBrowser FromText
            (string text, string sourceIdentifier = null)
            => new CompilerBrowser(() => Compiler.FromText(text, null, sourceIdentifier));

        public static CompilerBrowser FromFile
            (string fileName, CompilerParameters parameters = null)
            => new CompilerBrowser(() => Compiler.FromFile(fileName, parameters));

        readonly IDictionary<IFormalCodeItem, int> CodeToFunctionIndexCache =
            new Dictionary<IFormalCodeItem, int>();

        readonly ValueCache<Compiler> ParentCache;

        internal CompilerBrowser(Func<Compiler> parent) => ParentCache = new ValueCache<Compiler>(parent);

        public Source Source => Compiler.Source;

        public Compiler Compiler => ParentCache.Value;

        internal IExecutionContext ExecutionContext => Compiler;

        internal IEnumerable<Issue> Issues => Compiler.Issues;

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

        internal Syntax FormattingSyntax 
            => Cache.FomattingSyntax ?? (Cache.FomattingSyntax = new Syntax(Compiler.Syntax));

        internal Helper.Syntax Syntax => Cache.HelperSyntax ?? (Cache.HelperSyntax = GetHelperSyntax());

        Helper.Syntax GetHelperSyntax()
        {
            try
            {
                return new Helper.Syntax(Compiler.Syntax);
            }
            catch(Exception e)
            {
                $"Syntax: Unexpected {e} \nText:\n{Source.Data}".WriteLine();
                throw;
            }

            ;
        }

        public Token LocatePosition(int offset)
            => Token.LocatePosition(Syntax, offset);

        internal IEnumerable<Value> FindPosition(int offset)
        {
            var enumerable = LocatePosition(offset)
                .Syntax
                .ParentChainIncludingThis
                .Select(item => item.Target.Value.Target)
                .ToArray();

            var compileSyntaxs = enumerable
                .Where(item => item?.ResultCache.Any() ?? false);
            return compileSyntaxs;
        }

        public Token LocatePosition(SourcePosn current)
        {
            Tracer.Assert(current.Source == Source);
            return LocatePosition(current.Position);
        }

        internal FunctionType Function(int index)
            => Compiler.Root.Function(index);


        internal object FindFunction(IFormalCodeItem codeBase)
        {
            var result = FindFunctionIndex(codeBase);
            return result == null
                ? (object) Compiler.CodeContainer.Main.Data
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

        internal IEnumerable<Reni.TokenClasses.Syntax> FindAllBelongings(Helper.Syntax syntax)
            => Compiler.Syntax.Belongings(syntax.Target);

        internal string Reformat(IFormatter formatter = null, SourcePart targetPart = null) =>
            (formatter ?? new Formatting.Configuration().Create())
            .GetEditPieces(this, targetPart)
            .Combine(Syntax.Target.SourcePart);

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

        public IEnumerable<SourcePart> FindAllBelongings(Token open) => open.FindAllBelongings(this);

        Reni.TokenClasses.Syntax LocateActivePosition(int offset)
        {
            var token = Token.LocatePosition(Syntax, offset);
            if(token.IsComment || token.IsLineComment)
                return null;

            var tokenSyntax = token.Syntax.Target;
            var position = tokenSyntax.Token.Characters.Position;
            return position <= 0 ? null : tokenSyntax.Option.LocatePosition(position - 1);
        }

        internal string[] DeclarationOptions(int offset)
        {
            Ensure();
            return LocateActivePosition(offset)?.DeclarationOptions ?? new string[0];
        }

        internal IEnumerable<Edit> GetEditPieces
            (SourcePart sourcePart, IFormatter formatter = null)
            => (formatter ?? new Formatting.Configuration().Create())
                .GetEditPieces(this, sourcePart);

        public string FlatFormat(bool areEmptyLinesPossible) 
            => FormattingSyntax.FlatFormat( areEmptyLinesPossible);
    }
}