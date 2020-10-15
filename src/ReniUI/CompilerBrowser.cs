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
using ReniUI.Helper;

namespace ReniUI
{
    public sealed class CompilerBrowser : DumpableObject, ValueCache.IContainer
    {
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

        internal Helper.Syntax Syntax => this.CachedValue(GetSyntax);
        [Obsolete("",true)]
        internal Helper.SyntaxOld SyntaxOld => this.CachedValue(GetSyntaxOld);

        internal Formatting.BinaryTree FormattingBinary
            => this.CachedValue(() => new Formatting.BinaryTree(Compiler.BinaryTree));

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
            Tracer.Assert(current.Source == Source);
            return LocatePosition(current.Position);
        }

        public string FlatFormat(bool areEmptyLinesPossible)
            => FormattingBinary.FlatFormat(areEmptyLinesPossible);

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
                return new Helper.Syntax(Compiler.BinaryTree, getFlatSyntax: ()=>Compiler.Syntax);
            }
            catch(Exception e)
            {
                $"Syntax: Unexpected {e} \nText:\n{Source.Data}".Log();
                throw;
            }
        }

        Helper.SyntaxOld GetSyntaxOld()
        {
            try
            {
                return new Helper.SyntaxOld(Syntax);
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