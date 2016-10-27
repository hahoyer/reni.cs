﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Feature
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class TypeOperator : SuffixSyntaxToken
    {
        public const string TokenId = "type";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category, Parser.Value left)
        {
            if(category.HasType)
                return left
                    .Type(context)
                    .TypeForTypeOperator
                    .TypeType
                    .Result(category);

            return context
                .RootContext.VoidType.Result(category);
        }
    }
}