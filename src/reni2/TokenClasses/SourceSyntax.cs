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
        static int _nextObjectId;
        readonly Issue[] _issues;

        public SourceSyntax
            (SourceSyntax left, IToken token, SourceSyntax right, Syntax syntax, Issue[] issues)
            : base(_nextObjectId++)
        {
            Syntax = syntax;
            _issues = issues ?? new Issue[0];
            Left = left;
            Token = token;
            Right = right;
            AssertValid();
        }

        void AssertValid()
        {
            AssertValidSourceQueue();
        }

        void AssertValidSourceQueue()
        {
            if(Left != null)
                Tracer.Assert
                    (
                        Left.SourcePart.End == Token.SourcePart.Start,
                        () => Left.SourcePart.End.Span(Token.SourcePart.Start).NodeDump
                    );
            if(Right != null)
                Tracer.Assert
                    (
                        Token.SourcePart.End == Right.SourcePart.Start,
                        () => Token.SourcePart.End.Span(Right.SourcePart.Start).NodeDump
                    );
        }

        internal Issue[] Issues => (Left?.Issues).plus(_issues).plus(Right?.Issues);
        internal Syntax Syntax { get; }
        SourceSyntax Left { get; }
        internal IToken Token { get; }
        SourceSyntax Right { get; }

        SourcePart ISourcePart.All => SourcePart;

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart + Right?.SourcePart;
        [DisableDump]
        string FilePosition => Token.Characters.FilePosition;

        protected override string GetNodeDump() => base.GetNodeDump() + " " + SourcePart.Id.Quote();

        internal TokenInformation LocateToken(SourcePosn sourcePosn)
        {
            if(sourcePosn.IsEnd)
                return new SyntaxToken(this);

            if(sourcePosn < Token.SourcePart)
                return Left?.LocateToken(sourcePosn);

            if(sourcePosn < Token.Characters)
                return new UserInterface.WhiteSpaceToken
                    (Token.PrecededWith.Single(item => item.Characters.Contains(sourcePosn)));

            if(Token.Characters.Contains(sourcePosn))
                return new SyntaxToken(this);

            return Right?.LocateToken(sourcePosn);
        }
    }
}