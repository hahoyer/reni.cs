using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class PredefinedTokenFactory<TSourcePart> : ScannerTokenType<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        readonly ValueCache<FunctionCache<string, IParserTokenType<TSourcePart>>>
            PredefinedTokenClassesCache;

        protected PredefinedTokenFactory() => PredefinedTokenClassesCache =
            new ValueCache<FunctionCache<string, IParserTokenType<TSourcePart>>>(GetDictionary);

        /// <summary>
        ///     Override this method, when the dictionary requires a key different from occurence found in source,
        ///     for instance, when your language is not case sensitive or for names only some first characters are significant.
        ///     To register the names actually used, <see cref="IAliasKeeper" />.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Default implementation returns the id.</returns>
        public Func<string, string> GetTokenClassKeyFromToken {get; set;} = id => id;

        FunctionCache<string, IParserTokenType<TSourcePart>> GetDictionary()
            => new FunctionCache<string, IParserTokenType<TSourcePart>>
            (
                GetPredefinedTokenClasses().ToDictionary(item => GetTokenClassKeyFromToken(item.Id)),
                GetTokenClass
            );

        protected sealed override IParserTokenType<TSourcePart> GetParserTokenType(string id)
        {
            var key = GetTokenClassKeyFromToken(id);
            IParserTokenType<TSourcePart> result = PredefinedTokenClassesCache.Value[key];
            (result as IAliasKeeper)?.Add(id);
            return result;
        }

        protected abstract IEnumerable<IParserTokenType<TSourcePart>> GetPredefinedTokenClasses();
        protected abstract IParserTokenType<TSourcePart> GetTokenClass(string name);
    }

    /// <summary>
    ///     Use this interface at your <see cref="IParserTokenType&lt;TSourcePart&gt;" /> to register names that are acually
    ///     used for your token type.
    /// </summary>
    interface IAliasKeeper
    {
        /// <summary>
        ///     Method is called for every occurence
        /// </summary>
        /// <param name="id">the actual name version</param>
        void Add(string id);
    }
}