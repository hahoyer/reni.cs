using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Struct;
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
            var s = Syntax.Issues;
            var a = (Left?.Syntax?.Issues).plus(Right?.Syntax?.Issues);
            Tracer.Assert(a != null);
            var x = s.Merge(a, item => item);
            var n = x.Where(item => item.Item3 == null).ToArray();
            var lost = x.Where(item => item.Item2 == null).ToArray();
            Tracer.Assert(n.Length <= 1);
            Tracer.Assert(!lost.Any());
        }


        public Syntax Syntax { get; }
        SourceSyntax Left { get; }
        internal IToken Token { get; }
        SourceSyntax Right { get; }

        SourcePart ISourcePart.All => SourcePart;

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart + Right?.SourcePart;
        [DisableDump]
        string FilePosition => Token.Characters.FilePosition;

        [DisableDump]
        public bool IsKeyword
        {
            get
            {
                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        public bool IsIdentifier
        {
            get
            {
                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        public bool IsText
        {
            get
            {
                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        public bool IsNumber
        {
            get
            {
                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        public bool IsError
        {
            get
            {
                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        public bool IsBraceLike
        {
            get
            {
                NotImplementedMethod();
                return false;
            }
        }


        protected override string GetNodeDump() => SourcePart.Id;

        static bool _isInDump;

        protected override string Dump(bool isRecursion)
        {
            if(isRecursion)
                return "ObjectId=" + ObjectId;

            var isInContainerDump = CompoundSyntax.IsInContainerDump;
            CompoundSyntax.IsInContainerDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = GetNodeDump();
            if(!ParsedSyntax.IsDetailedDumpRequired)
                return result;
            if(!isInDump && SourcePart.Source.IsPersistent)
                result += FilePosition;
            if(isInContainerDump)
                result += " ObjectId=" + ObjectId;
            else
                result += "\n" + base.Dump(false);
            CompoundSyntax.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        internal TokenInformation LocateToken(SourcePosn sourcePosn)
        {
            var token = Token;
            if(token.Characters.Contains(sourcePosn))
                return new SyntaxToken(this);

            if(!SourcePart.Contains(sourcePosn))
                return null;

            NotImplementedMethod(sourcePosn);

            var whiteSpaceToken = token.PrecededWith.First
                (item => item.Characters.Contains(sourcePosn));
            return new UserInterface.WhiteSpaceToken(whiteSpaceToken);
        }

        [DisableDump]
        internal string DumpPrintText => SourcePart.Id;
    }
}