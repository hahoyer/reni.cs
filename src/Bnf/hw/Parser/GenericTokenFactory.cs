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
    ///     <see cref="BelongsToFactory" />.
    /// </summary>
    /// <typeparam name="TSourcePart"></typeparam>
    abstract class GenericTokenFactory<TSourcePart> : PredefinedTokenFactory<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        static IEnumerable<IParserTokenType<TSourcePart>> CreateInstance(Type type)
        {
            var variants = type.GetAttributes<VariantAttribute>(true).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance<TSourcePart>(type));

            return new[] {SpecialTokenClass(type)};
        }

        static IParserTokenType<TSourcePart> SpecialTokenClass(Type type)
            => (IParserTokenType<TSourcePart>) Activator.CreateInstance(type);

        /// <summary>
        ///     Tokenclasses that have been obtained from current assembly
        /// </summary>
        public readonly IEnumerable<IParserTokenType<TSourcePart>> PredefinedTokenClasses;

        protected abstract IParserTokenType<TSourcePart> NewSymbol(string name);

        [EnableDump]
        readonly string Title;

        readonly List<IParserTokenType<TSourcePart>> UserSymbols = new List<IParserTokenType<TSourcePart>>();

        /// <summary />
        /// <param name="newSymbol">Function that should be called, when any well formed, but yet unknown symbol is found.</param>
        /// <param name="title">Optional name for use in trace log.</param>
        public GenericTokenFactory(Func<string, IParserTokenType<TSourcePart>> newSymbol, string title = null)
        {
            Title = title;
            PredefinedTokenClasses = GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);
        }

        /// <summary>
        /// Complete list of token classes seen or predefined. Will probaly increase during compilation.
        /// </summary>
        [DisableDump]
        public IEnumerable<IParserTokenType<TSourcePart>> AllTokenClasses
            => PredefinedTokenClasses.Concat(UserSymbols);

        protected sealed override IParserTokenType<TSourcePart> GetTokenClass(string name)
        {
            var result = NewSymbol(name);
            UserSymbols.Add(result);
            return result;
        }

        protected sealed override IEnumerable<IParserTokenType<TSourcePart>> GetPredefinedTokenClasses()
            => PredefinedTokenClasses;

        bool BelongsToFactory(Type type)
        {
            var thisType = GetType();
            return type.Is<ScannerTokenType>() &&
                   !type.IsAbstract &&
                   type
                       .GetAttributes<BelongsToAttribute>(true)
                       .Any(attr => thisType.Is(attr.TokenFactory));
        }
    }
}