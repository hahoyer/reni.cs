using System.Collections.Concurrent;

namespace ReniUI;

sealed class Persister : DumpableObject
{
    sealed class Member<T> : DumpableObject, IMember
    {
        public readonly Action<T> Load;
        public readonly Func<T> Store;
        readonly string Name;
        readonly IPersistenceHandler<T> Handler;

        public Member(string name, Action<T> load, Func<T> store, IPersistenceHandler<T> handler)
        {
            Name = name;
            Load = load;
            Store = store;
            Handler = handler;
        }

        void IMember.Load()
        {
            var value = Handler.Get(Name);
            if(value != null)
                Load(value);
        }

        void IMember.Store() => Handler.Set(Name, Store());
    }

    interface IMember
    {
        void Load();
        void Store();
    }

    readonly IDictionary<string, IMember> Members = new ConcurrentDictionary<string, IMember>();

    readonly SmbFile Handle;

    [EnableDump]
    string FileName => Handle.FullName;

    internal Persister(SmbFile handle) => Handle = handle;

    public void Register<T>(string name, Action<T> load, Func<T> store)
        =>
            Members.Add
            (
                name,
                new Member<T>(name, load, store, new FilePersistenceHandler<T>(FileName)));

    public void Load()
    {
        foreach(var member in Members)
            member.Value.Load();
    }

    public void Store()
    {
        foreach(var member in Members)
            member.Value.Store();
    }

    public void Store(string name) => Members[name].Store();
}

interface IPersistenceHandler<T>
{
    T Get(string name);
    void Set(string name, T value);
}

sealed class FilePersistenceHandler<T> : FilePersistenceHandler, IPersistenceHandler<T>
{
    public FilePersistenceHandler(string fileName)
        : base(fileName) { }

    T IPersistenceHandler<T>.Get(string name) => (T)Get(typeof(T), name);

    void IPersistenceHandler<T>.Set(string name, T value) => Set(name, value);
}

abstract class FilePersistenceHandler : DumpableObject
{
    [EnableDump]
    readonly string FileName;

    protected FilePersistenceHandler(string fileName) => FileName = fileName;

    protected object Get(Type type, string name)
    {
        var text = ToSmbFile(name).String;
        if(text == null)
            return null;

        if(type == typeof(Tuple<int, int>))
        {
            var values = text
                .Substring(1, text.Length - 2)
                .Split(',')
                .Take(2)
                .Select(int.Parse)
                .ToArray();
            return new Tuple<int, int>(values[0], values[1]);
        }

        NotImplementedMethod(type, name);
        return null;
    }

    SmbFile ToSmbFile(string name) => FileName.ToSmbFile() / name;

    protected void Set(string name, object value) => ToSmbFile(name).String = value.ToString();
}