﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class LeftParenthesis : TokenClass, ITokenClassWithId
    {
        public static string Id(int level) => "\0{[(".Substring(level, 1);

        public LeftParenthesis(int level) { Level = level; }

        int Level { get; }

        string ITokenClassWithId.Id => Id(Level);

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => new LeftParenthesisSyntax(Level, token, right);
        protected override Syntax Terminal(SourcePart token)
            => new LeftParenthesisSyntax(Level, token, null);
    }
}