using System.IO;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal sealed class FileSource : Source
    {
        private readonly string _physicalName;

        internal FileSource(string physicalName)
        {
            _physicalName = physicalName;
        }

        internal override Files Files(NoCaseStringDictionary<Config> config)
        {
            Files result = base.Files(config);
            result |= FilesAsRoot(_physicalName, config);
            return result;
        }

        internal string PhysicalName { get { return _physicalName; } }

        private static Files FilesAsRoot(string physicalName, NoCaseStringDictionary<Config> config)
        {
            Files result = new Files();

            DirectoryInfo thisDirectoryInfo = new DirectoryInfo(physicalName + "\\");
            if (!thisDirectoryInfo.Exists)
                return result;

            DirectoryInfo[] directoryInfos = thisDirectoryInfo.GetDirectories();
            foreach (DirectoryInfo directoryInfo in directoryInfos)
                result.AddWithOr(directoryInfo.Name, Directory.Create(directoryInfo.Name, physicalName, config));

            FileInfo[] fileInfos = thisDirectoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
                result.AddWithOr(fileInfo.Name, new DataFile(config[fileInfo.Name]));

            return result;
        }
    }
}