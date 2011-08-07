using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.TokenClasses
{
    internal sealed class Number : Terminal
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            var bitsConst = BitsConst.Convert(token.Name);
            return TypeBase
                .UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst));
        }

        public static Int64 ToInt64(TokenData token) { return BitsConst.Convert(token.Name).ToInt64(); }
    }
}