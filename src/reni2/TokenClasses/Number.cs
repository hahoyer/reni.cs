﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;

namespace Reni.TokenClasses
{
    sealed class Number : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, SourcePart token) => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Name));
        public static Int64 ToInt64(SourcePart token) => BitsConst.Convert(token.Name).ToInt64();
    }
}