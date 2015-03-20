using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    abstract class TokenFactory : TokenFactory<TokenClass, SourceSyntax>
    {
        /// <summary>
        ///     Creates the main token classes.
        /// </summary>
        /// <returns> </returns>
        protected override IEnumerable<TokenClass> GetPredefinedTokenClasses()
            => TokenClasses;

        IEnumerable<TokenClass> TokenClasses
            => GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);

        IEnumerable<TokenClass> CreateInstance(System.Type type)
        {
            var variants = type.GetAttributes<Variant>(false).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance(type));
            return new[] {SpecialTokenClass(type)};
        }

        protected virtual TokenClass SpecialTokenClass(System.Type type)
            => (TokenClass) Activator.CreateInstance(type);

        bool BelongsToFactory(System.Type type)
        {
            return type.Is<TokenClass>() && !type.IsAbstract && type
                .GetAttributes<BelongsTo>(true)
                .Any(attr => GetType().Is(attr.TokenFactory));
        }
    }
}