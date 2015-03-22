using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.UserInterface;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class SourceSyntax : DumpableObject, ISourcePart
    {
        public SourceSyntax(Syntax syntax, SourceSyntax left, IToken token, SourceSyntax right)
        {
            Syntax = syntax;
            Left = left;
            Token = token;
            Right = right;
            AssertValid();
        }

        void AssertValid()
        {
            var currentIssues = Syntax.Issues;
            var formerIssues = (Left?.Syntax?.Issues).plus(Right?.Syntax?.Issues);
            Tracer.Assert(formerIssues != null);
            var combinations = currentIssues.Merge(formerIssues, item => item).ToArray();
            var newIssues = combinations.Where(item => item.Item3 == null).ToArray();
            var lostIssues = combinations.Where(item => item.Item2 == null).ToArray();
            Tracer.Assert
                (
                    !lostIssues.Any(),
                    () =>
                        "\n" +
                            Tracer.Dump(lostIssues.Select(item => item.Item3).ToArray()) +
                            "\n" +
                            Syntax.Dump()
                );
        }

        internal Syntax Syntax { get; }
        SourceSyntax Left { get; }
        internal IToken Token { get; }
        SourceSyntax Right { get; }

        SourcePart ISourcePart.All => SourcePart;

        [EnableDump]
        Issue[] Issues => Syntax.Issues.ToArray();

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart + Right?.SourcePart;
        [DisableDump]
        string FilePosition => Token.Characters.FilePosition;

        protected override string GetNodeDump() => base.GetNodeDump() + " " + SourcePart.Id.Quote();

        internal TokenInformation LocateToken(SourcePosn sourcePosn)
        {
            if(sourcePosn.IsEnd)
                return new SyntaxToken(this);

            if (sourcePosn < Token.SourcePart)
                return Left?.LocateToken(sourcePosn);

            if(sourcePosn < Token.Characters)
                return new UserInterface.WhiteSpaceToken
                    (Token.PrecededWith.Single(item => item.Characters.Contains(sourcePosn)));

            if (Token.Characters.Contains(sourcePosn))
                return new SyntaxToken(this);

            return Right?.LocateToken(sourcePosn);
        }
    }
}