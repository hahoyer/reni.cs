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

        internal CompilerBrowser(Func<Compiler> parent)
        {
            ParentCache = new ValueCache<Compiler>(parent);
        }

        public Source Source => Compiler.Source;

        Compiler Compiler => ParentCache.Value;

        internal IExecutionContext ExecutionContext => Compiler;

        internal IEnumerable<Issue> Issues => Compiler.Issues;

        public Token LocatePosition(int offset)
            => Token.LocatePosition(Compiler.SourceSyntax, offset);

        internal IEnumerable<CompileSyntax> FindPosition(int offset)
        {
            var enumerable = LocatePosition(offset)
                .SourceSyntax
                .ParentChainIncludingThis
                .Select(item => item.ToCompiledSyntax.Value)
                .ToArray();

            var compileSyntaxs = enumerable
                .Where(item => item?.ResultCache.Any()??false);

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
            int result;
            if(CodeToFunctionIndexCache.TryGetValue(codeBase, out result))
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

        internal IEnumerable<SourceSyntax> FindAllBelongings(SourceSyntax sourceSyntax)
            => Compiler.SourceSyntax.Belongings(sourceSyntax);

        internal string Reformat(IFormatter formatter)
            => formatter.Reformat(Compiler.SourceSyntax, Compiler.SourceSyntax.SourcePart);

        public SourceSyntax Locate(SourcePart span)
        {
            var result = Compiler.SourceSyntax.Locate(span);
            if(result != null)
                return result;

            Tracer.TraceBreak();
            NotImplementedMethod(span);
            return null;
        }

        internal void Ensure() => Compiler.Issues.ToArray();

        internal void Execute(DataStack dataStack) => Compiler.ExecuteFromCode(dataStack);

        public IEnumerable<SourcePart> FindAllBelongings(Token open) => open.FindAllBelongings(this);

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

        internal SourceSyntax SourceSyntax => Compiler.SourceSyntax;
    }
}