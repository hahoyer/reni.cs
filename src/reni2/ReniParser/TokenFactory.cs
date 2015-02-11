using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    abstract class TokenFactory : TokenFactory<TokenClass, Syntax>
    {
        /// <summary>
        ///     Creates the main token classes.
        /// </summary>
        /// <returns> </returns>
        protected override IDictionary<string, TokenClass> GetPredefinedTokenClasses()
            => TokenClasses.ToDictionary(t => t.Id, t => (TokenClass) t);

        IEnumerable<ITokenClassWithId> TokenClasses
            => GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);

        IEnumerable<ITokenClassWithId> CreateInstance(System.Type type)
        {
            var variants = type.GetAttributes<Variant>(false).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance(type));
            return new[] {SpecialTokenClass(type)};
        }

        protected virtual ITokenClassWithId SpecialTokenClass(System.Type type)
            => (ITokenClassWithId) Activator.CreateInstance(type);

        bool BelongsToFactory(System.Type type)
        {
            if(type.Is<ITokenClassWithId>())
                return type
                    .GetAttributes<BelongsTo>(true)
                    .Any(attr => GetType().Is(attr.TokenFactory));
            return false;
        }
    }
}