using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal sealed class Config: Dumpable
    {
        private NoCaseStringDictionary<Config> _deeperSources = new NoCaseStringDictionary<Config>();
        private Sources _sources = new Sources();

        internal Config this[string name] { get { return _deeperSources.Find(name, delegate { return new Config(); }); } }

        internal Files Files()
        {
            return _sources.Files(_deeperSources);
        }

        internal void Add(Source source)
        {
            _sources.Add(source);
        }

        internal void Add(Config config)
        {
            _sources.Add(config._sources);
            string[] keys = config._deeperSources.Keys;
            for (int i = 0; i < keys.Length; i++)
                AddWithOr(keys[i], config._deeperSources[keys[i]]);
        }

        internal Sources Sources { get { return _sources; } }
        internal NoCaseStringDictionary<Config> DeeperSources { get { return _deeperSources; } }

        internal void AddWithOr(string name, Config otherConfig)
        {
            if (_deeperSources.ContainsKey(name))
                _deeperSources[name].Add(otherConfig);
            else
                _deeperSources.Add(name, otherConfig);
        }
    }
}