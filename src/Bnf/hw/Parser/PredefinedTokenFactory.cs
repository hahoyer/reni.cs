using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class PredefinedTokenFactory : DumpableObject, ITokenTypeFactory
    {
        readonly ValueCache<FunctionCache<string, ITokenType>>
            PredefinedTokenClassesCache;

        protected PredefinedTokenFactory() => PredefinedTokenClassesCache =
            new ValueCache<FunctionCache<string, ITokenType>>(GetDictionary);

        /// <summary>
        ///     Override this method, when the dictionary requires a key different from occurence found in source,
        ///     for instance, when your language is not case sensitive or for names only some first characters are significant.
        ///     To register the names actually used, <see cref="IAliasKeeper" />.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Default implementation returns the id.</returns>
        protected virtual string GetTokenClassKeyFromToken(string id) => id;

        FunctionCache<string, ITokenType> GetDictionary()
            => new FunctionCache<string, ITokenType>
            (
                GetPredefinedTokenClasses().ToDictionary(item => GetTokenClassKeyFromToken(item.Value)),
                GetTokenClass
            );

        ITokenType ITokenTypeFactory.Get(string id)
        {
            var key = GetTokenClassKeyFromToken(id);
            ITokenType result = PredefinedTokenClassesCache.Value[key];
            (result as IAliasKeeper)?.Add(id);
            return result;
        }

        protected abstract IEnumerable<ITokenType> GetPredefinedTokenClasses();
        protected abstract ITokenType GetTokenClass(string name);
    }

    /// <summary>
    ///     Use this interface at your <see cref="IPriorityParserTokenType{TTreeItem}" /> to register names that are acually
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