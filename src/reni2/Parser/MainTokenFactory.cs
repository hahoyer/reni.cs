﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Formatting;
using Reni.Numeric;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class MainTokenFactory : TokenFactory
    {
        static PrioTable PrioTable
        {
            get
            {
                var result = PrioTable.Left(PrioTable.Any);
                result += PrioTable.Left
                    (
                        AtToken.TokenId,
                        "_N_E_X_T_",
                        ToNumberOfBase.TokenId
                    );

                result += PrioTable.Left(ConcatArrays.TokenId, ConcatArrays.MutableId);

                result += PrioTable.Left(Star.TokenId, Slash.TokenId, "\\");
                result += PrioTable.Left(Plus.TokenId, Minus.TokenId);

                result += PrioTable.Left
                    (
                        CompareOperation.TokenId(),
                        CompareOperation.TokenId(canBeEqual: true),
                        CompareOperation.TokenId(false),
                        CompareOperation.TokenId(false, true)
                    );
                result += PrioTable.Left
                    (EqualityOperation.TokenId(false), EqualityOperation.TokenId());

                result += PrioTable.Right(ReassignToken.TokenId);

                result += PrioTable.Right(ThenToken.TokenId);
                result += PrioTable.Right(ElseToken.TokenId);

                result += PrioTable.Right(Exclamation.TokenId);
                result += PrioTable.Left
                    (
                        Function.TokenId(),
                        Function.TokenId(true),
                        Function.TokenId(isMetaFunction: true));
                result += PrioTable.Right(Colon.TokenId);
                result += PrioTable.Right(List.TokenId(0));
                result += PrioTable.Right(List.TokenId(1));
                result += PrioTable.Right(List.TokenId(2));
                result += PrioTable.Right(Cleanup.TokenId);
                result += PrioTable.Right(PrioTable.Error);

                result += PrioTable.BracketParallels
                    (
                        new[]
                        {
                            LeftParenthesis.TokenId(3),
                            LeftParenthesis.TokenId(2),
                            LeftParenthesis.TokenId(1),
                            PrioTable.BeginOfText
                        },
                        new[]
                        {
                            RightParenthesis.TokenId(3),
                            RightParenthesis.TokenId(2),
                            RightParenthesis.TokenId(1),
                            PrioTable.EndOfText
                        }
                    );

                //Tracer.FlaggedLine("\n"+x.ToString());
                return result;
            }
        }


        public readonly IParser<SourceSyntax> Parser;

        readonly ISubParser<SourceSyntax> _declarationSyntaxSubParser;
        readonly PrioParser<SourceSyntax> _declarationSyntaxParser;

        public MainTokenFactory
            (Func<ITokenFactory<SourceSyntax>, IScanner<SourceSyntax>> getScanner)
        {
            Parser = new PrioParser<SourceSyntax>
                (PrioTable, getScanner(this), new LeftParenthesis(0));
            _declarationSyntaxParser = new PrioParser<SourceSyntax>
                (
                DeclarationTokenFactory.PrioTable,
                getScanner(new DeclarationTokenFactory())
                ,
                null);
            _declarationSyntaxSubParser = new SubParser<SourceSyntax>
                (_declarationSyntaxParser, Pack);
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

        protected override ScannerTokenClass SpecialTokenClass(System.Type type)
        {
            if(type == typeof(Exclamation))
                return new Exclamation(_declarationSyntaxSubParser);
            return base.SpecialTokenClass(type);
        }

        static IType<SourceSyntax> Pack(SourceSyntax options) => new ExclamationBoxToken(options);

        protected override ScannerTokenClass GetEndOfText() => new RightParenthesis(0);
        protected override ScannerTokenClass GetNumber() => new Number();
        protected override ScannerTokenClass GetTokenClass(string name) => new UserSymbol(name);

        protected override ScannerTokenClass GetError(Match.IError message)
            => new ScannerSyntaxError(message);

        protected override ScannerTokenClass GetText() => new Text();
    }


    sealed class ScannerSyntaxError : ScannerTokenClass, IType<SourceSyntax>, ITokenClass
    {
        readonly IssueId _issue;

        public ScannerSyntaxError(Match.IError message)
        {
            _issue = Lexer.Parse(message);
            StopByObjectIds(81);
        }

        string IType<SourceSyntax>.PrioTableId => Id;

        public override string Id => "<error>";

        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
            => new SourceSyntax(left, this, token, right, GetResult);

        Checked<Syntax> GetResult(Syntax left, IToken token, Syntax right)
            =>
                new Checked<Syntax>
                    (ListSyntax.Create(left, right), _issue.CreateIssue(token.Characters));
    }
}