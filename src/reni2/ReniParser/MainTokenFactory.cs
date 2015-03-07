using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Numeric;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class MainTokenFactory : TokenFactory
    {
        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.Left
                    (
                        AtToken.Id,
                        "_N_E_X_T_",
                        ToNumberOfBase.Id
                    );

                x += PrioTable.Left(ConcatArrays.Id, ConcatArrays.MutableId);

                x += PrioTable.Left(Star.Id, Slash.Id, "\\");
                x += PrioTable.Left(Plus.Id, Minus.Id);

                x += PrioTable.Left
                    (
                        CompareOperation.Id(),
                        CompareOperation.Id(canBeEqual: true),
                        CompareOperation.Id(false),
                        CompareOperation.Id(false, true)
                    );
                x += PrioTable.Left(EqualityOperation.Id(false), EqualityOperation.Id());

                x += PrioTable.Right(ReassignToken.Id);

                x = x.ThenElseLevel(ThenToken.Id, ElseToken.Id);
                x += PrioTable.Right(Exclamation.Id);
                x += PrioTable.Left
                    (Function.Id(), Function.Id(true), Function.Id(isMetaFunction: true));
                x += PrioTable.Right(Colon.Id);
                x += PrioTable.Right(List.Id(0));
                x += PrioTable.Right(List.Id(1));
                x += PrioTable.Right(List.Id(2));
                x = x.ParenthesisLevelLeft
                    (
                        new[] {LeftParenthesis.Id(1), LeftParenthesis.Id(2), LeftParenthesis.Id(3)},
                        new[]
                        {
                            RightParenthesis.Id(1),
                            RightParenthesis.Id(2),
                            RightParenthesis.Id(3)
                        }
                    );
                //x.Correct("(", PrioTable.Any, '-');
                //x.Correct("[", PrioTable.Any, '-');
                //x.Correct("{", PrioTable.Any, '-');

                x += PrioTable.Right(PrioTable.Error);

                x = x.ParenthesisLevelLeft
                    (new[] {PrioTable.BeginOfText}, new[] {PrioTable.EndOfText});

                //Tracer.FlaggedLine("\n"+x.ToString());
                return x;
            }
        }


        public readonly IParser<Syntax> Parser;

        readonly ISubParser<Syntax> _declarationSyntaxSubParser;
        readonly PrioParser<Syntax> _declarationSyntaxParser;

        public MainTokenFactory()
        {
            Parser = new PrioParser<Syntax>
                (PrioTable, new Scanner<Syntax>(ReniLexer.Instance), this);
            _declarationSyntaxParser = new PrioParser<Syntax>
                (
                DeclarationTokenFactory.PrioTable,
                new Scanner<Syntax>(ReniLexer.Instance),
                new DeclarationTokenFactory()
                );
            _declarationSyntaxSubParser = new SubParser<Syntax>(_declarationSyntaxParser, Pack);
        }

        public bool Trace
        {
            get { return Parser.Trace; }
            set
            {
                Parser.Trace = value;
                _declarationSyntaxParser.Trace = value;
            }
        }

        protected override ITokenClassWithId SpecialTokenClass(System.Type type)
        {
            if(type == typeof(Exclamation))
                return new Exclamation(_declarationSyntaxSubParser);
            return base.SpecialTokenClass(type);
        }

        static IType<Syntax> Pack(Syntax options) => new SyntaxBoxToken(options);

        protected override TokenClass GetEndOfText() => new EndToken();
        protected override TokenClass GetNumber() => new Number();
        protected override TokenClass GetTokenClass(string name) => new UserSymbol(name);
        protected override TokenClass GetError(Match.IError message) => new SyntaxError(message);
        protected override TokenClass GetText() => new Text();
    }


    sealed class SyntaxError : TokenClass
    {
        readonly IssueId _issue;
        public SyntaxError(IssueId issue) { _issue = issue; }
        public SyntaxError(Match.IError message) { _issue = ReniLexer.Parse(message); }
        protected override Syntax Terminal(SourcePart token)
            => new CompileSyntaxError(_issue, token, null);
        protected override Syntax Suffix(Syntax left, SourcePart token)
            => left.SyntaxError(_issue, token, null);
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => left.SyntaxError(_issue, token, right, null);
    }
}