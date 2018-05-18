﻿using System.Collections.Generic;using System.Linq;using Bnf.Base;using Bnf.Forms;using Bnf.Parser;using hw.DebugFormatter;using hw.Helper;using hw.Parser;using hw.Scanner;namespace Bnf.StructuredText{    sealed class Compiler : DumpableObject, IResultFactory<ISyntax>    {        static readonly IDictionary<string, IExpression> Definitions            = Bnf.Compiler.FromText(BnfDefinitions.Text)                .Statements;        public static Compiler FromText(string programText) => new Compiler(new Source(programText));        static TwoLayerScanner CreateScanner(IMatch parserLiteralMatch)        {            var factory = new ScannerTokenFactory(Definitions, parserLiteralMatch);            var cachingFactory = new CachingTokenFactory(factory);            return new TwoLayerScanner(cachingFactory);        }        public Context RootContext = Context.Root;        readonly Source Source;        IParser<ISyntax> ParserCache;        ISyntax SyntaxCache;        Compiler(Source source) => Source = source;        ISyntax IResultFactory<ISyntax>.EmptyRepeat => new EmptySyntax();        ISyntax IResultFactory<ISyntax>.LiteralMatch(TokenGroup token)            => new Singleton(token);        ISyntax IResultFactory<ISyntax>.Sequence(IEnumerable<ISyntax> data)        {            var sequence = data.SelectMany(item => item.ToSequence).ToArray();            switch(sequence.Length)            {                case 0: return new EmptySyntax();                case 1: return sequence[0];                default: return new SequenceSyntax(sequence);            }        }        IParser<ISyntax> Parser => ParserCache ?? (ParserCache = GetParser());        [DisableDump]        public ISyntax Syntax => SyntaxCache ?? (SyntaxCache = Parse(Source + 0));        IParser<ISyntax> GetParser()        {            var definitions = new Definitions<ISyntax>(Definitions, "structured_text");            definitions.Register(this);            var scanner = CreateScanner(definitions.ParserLiteralMatch);            return new BnfParser<ISyntax>(scanner,definitions,this);        }        ISyntax Parse(SourcePosn sourcePosn) => Parser.Execute(sourcePosn);    }    sealed class SequenceSyntax : DumpableObject, ISyntax    {        [EnableDump]        readonly ISyntax[] Data;        public SequenceSyntax(IEnumerable<ISyntax> data)        {            Tracer.Assert(!data.OfType<SequenceSyntax>().Any());            Data = data.ToArray();        }        SourcePart ISourcePartProxy.All => Data.Select(d => d.All).Aggregate();        int IParseSpan.Value => Data.Select(i => i.Value).Sum();        IEnumerable<ISyntax> ISyntax.ToSequence => Data;    }    sealed class EmptySyntax : DumpableObject, ISyntax    {        SourcePart ISourcePartProxy.All => null;        int IParseSpan.Value => 0;        IEnumerable<ISyntax> ISyntax.ToSequence {get {yield break;}}    }    interface ISyntax : ISourcePartProxy, IParseSpan    {        IEnumerable<ISyntax> ToSequence {get;}    }    sealed class Singleton : DumpableObject, ISyntax    {        readonly TokenGroup Token;        public Singleton(TokenGroup token) => Token = token;        SourcePart ISourcePartProxy.All => Token.SourcePart;        int IParseSpan.Value => 1;        IEnumerable<ISyntax> ISyntax.ToSequence {get {yield return this;}}        [EnableDump]        string Id => Token.Characters.Id;    }    abstract class InvalidDeclaration : DumpableObject, IHiearachicalItem<ISyntax>    {        ISyntax IHiearachicalItem<ISyntax>.Parse(IParserCursor source, Forms.IContext<ISyntax> context) => null;        string IHiearachicalItem<ISyntax>.Name => Name;        protected abstract string Name {get;}    }    [BelongsTo(typeof(Compiler))]    sealed class NonRetentiveVarDeclaration : InvalidDeclaration    {        protected override string Name => "non_retentive_var_declarations";    }    [BelongsTo(typeof(Compiler))]    sealed class Nil : InvalidDeclaration    {        protected override string Name => "NIL";    }}