using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal sealed class Files : Dumpable 
    {
        private NoCaseStringDictionary<File> _data;

        internal Files()
        {
            _data = new NoCaseStringDictionary<File>();
        }

        private Files(NoCaseStringDictionary<File> data)
        {
            _data = data.Clone;
        }

        static public Files operator |(Files a, Files b)
        {
            return a.Or(b);
        }

        private int Count { get { return _data.Count; } }

        internal string[] Names { get { return _data.Keys; } }

        private Files Or(Files other)
        {                                            
            Files result = new Files(_data);
            result.AddWithOr(other);
            return result;
        }

        internal Files Or(string name, File other)
        {
            Files result = new Files(_data);
            result.AddWithOr(name, other);
            return result;
        }

        internal void AddWithOr(Files other)
        {
            string[] keys = other._data.Keys;
            for (int i = 0; i < keys.Length; i++)
                AddWithOr(keys[i], other._data[keys[i]]);
        }

        internal void AddWithOr(string name, File otherFile)
        {
            if (_data.ContainsKey(name)) 
                _data[name].Add(otherFile);
            else 
                _data.Add(name, otherFile);
        }

        internal File this[string name] { get { return _data[name]; } }
    }
}