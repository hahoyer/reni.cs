using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal sealed class Directory : File
    {
        internal Directory(Config config)
            : base(config)
        {
        }

        internal string[] FileNames { get { return Files.Names; } }

        [DumpData(false)]
        internal Files Files { get { return Config.Files(); } }

        internal static Directory Create(string name, string physicalName, NoCaseStringDictionary<Config> configs)
        {
            Config config = new Config();
            config.Add(new FileSource(ConcatPhysicalPathName(physicalName,name)));
            Config inheritedConfig = configs[name];
            if(inheritedConfig != null)
                config.Add(inheritedConfig);

            return new Directory(config);
        }

        internal static string[] Concat(string[] path, string name)
        {
            List<string> result = new List<string>(path);
            result.Add(name);
            return result.ToArray();
        }

        internal static string CreatePhysicalName(string[] path)
        {
            string result = "";
            for (int i = 0; i < path.Length; i++)
                result = ConcatPhysicalPathName(result, path[i]);
            return result;
        }

        internal static string ConcatPhysicalPathName(string path, string name)
        {
            return path + "\\" + name;
        }

    }
}