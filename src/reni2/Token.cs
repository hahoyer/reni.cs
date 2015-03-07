using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni
{
    public abstract class Token : DumpableObject
    {
        public abstract SourcePart SourcePart { get; }
        [DisableDump]
        public int StartPosition => SourcePart.Position;
        [DisableDump]
        public int Length => SourcePart.Length;
        [DisableDump]
        public virtual bool IsKeyword => false;
        [DisableDump]
        public virtual bool IsIdentifier => false;
        [DisableDump]
        public virtual bool IsText => false;
        [DisableDump]
        public virtual bool IsNumber => false;
        [DisableDump]
        public virtual bool IsComment => false;
        [DisableDump]
        public virtual bool IsLineComment => false;
        [DisableDump]
        public virtual bool IsWhiteSpace => false;
        [DisableDump]
        public virtual bool IsBraceLike => false;
        [DisableDump]
        public virtual bool IsError => false;
        [DisableDump]
        public int Id => ObjectId;
    }

    sealed class SyntaxToken : Token
    {
        internal SyntaxToken(Syntax syntax) { Syntax = syntax; }

        Syntax Syntax { get; }
        public override SourcePart SourcePart => Syntax.Token;
        public override bool IsKeyword => Syntax.IsKeyword;
        public override bool IsIdentifier => Syntax.IsIdentifier;
        public override bool IsText => Syntax.IsText;
        public override bool IsNumber => Syntax.IsNumber;
        public override bool IsError => Syntax.IsError;
    }

    sealed class SpecialToken : Token
    {
        sealed public class Type
        {
            public static readonly Type WhiteSpace = new Type();
            public static readonly Type LineComment = new Type();
            public static readonly Type Comment = new Type();
            public static readonly Type Error = new Type();
        }

        readonly Type _type;
        public SpecialToken(SourcePart sourcePart, Type type)
        {
            _type = type;
            SourcePart = sourcePart;
        }

        public override SourcePart SourcePart { get; }
        public override bool IsComment => _type == Type.Comment;
        public override bool IsLineComment => _type == Type.LineComment;
        public override bool IsWhiteSpace => _type == Type.WhiteSpace;
        public override bool IsError => _type == Type.Error;
    }
}