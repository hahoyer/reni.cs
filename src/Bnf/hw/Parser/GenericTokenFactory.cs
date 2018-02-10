using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    /// <summary>
    ///     Tokenfactory, that obains token classes from current assembly.
    ///     At constuctor time, all classes are registered, that fulfil the conditions stated in function
    ///     <see cref="Extension.GetBelongings{T}" />.
    /// </summary>
    abstract class GenericTokenFactory : PredefinedTokenFactory
    {
        static IEnumerable<ITokenType> CreateInstance(Type type)
        {
            VariantAttribute[] variants = type.GetAttributes<VariantAttribute>(true).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance(type));

            return new[] {SpecialTokenClass(type)};
        }

        static ITokenType SpecialTokenClass(Type type)
            => (ITokenType) Activator.CreateInstance(type);

        /// <summary>
        ///     Tokenclasses that have been obtained from current assembly
        /// </summary>
        public readonly IEnumerable<ITokenType> PredefinedTokenClasses;

        [EnableDump]
        readonly string Title;

        readonly List<ITokenType> UserSymbols = new List<ITokenType>();

        /// <summary />
        /// <param name="title">Optional name for use in trace log.</param>
        public GenericTokenFactory(string title = null)
        {
            Title = title;
            PredefinedTokenClasses = GetType()
                .GetBelongingTypes<ITokenType>()
                .SelectMany(CreateInstance);
        }

        /// <summary>
        ///     Complete list of token classes seen or predefined. Will probaly increase during compilation.
        /// </summary>
        [DisableDump]
        public IEnumerable<ITokenType> AllTokenClasses
            => PredefinedTokenClasses.Concat(UserSymbols);

        protected abstract ITokenType NewSymbol(string name);

        protected sealed override ITokenType GetTokenClass(string name)
        {
            var result = NewSymbol(name);
            UserSymbols.Add(result);
            return result;
        }

        protected sealed override IEnumerable<ITokenType> GetPredefinedTokenClasses()
            => PredefinedTokenClasses;
    }
}