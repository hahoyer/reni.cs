using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    abstract class TokenFactory : TokenFactory<ScannerTokenClass, Syntax>
    {
        /// <summary>
        ///     Creates the main token classes.
        /// </summary>
        /// <returns> </returns>
        protected override IEnumerable<ScannerTokenClass> GetPredefinedTokenClasses()
            => TokenClasses;

        IEnumerable<ScannerTokenClass> TokenClasses
            => GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);

        IEnumerable<ScannerTokenClass> CreateInstance(System.Type type)
        {
            var variants = type.GetAttributes<Variant>(false).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance(type));
            return new[] {SpecialTokenClass(type)};
        }

        protected virtual ScannerTokenClass SpecialTokenClass(System.Type type)
            => (TokenClass) Activator.CreateInstance(type);

        bool BelongsToFactory(System.Type type)
        {
            return type.Is<ScannerTokenClass>() && !type.IsAbstract && type
                .GetAttributes<BelongsTo>(true)
                .Any(attr => GetType().Is(attr.TokenFactory));
        }
    }
}