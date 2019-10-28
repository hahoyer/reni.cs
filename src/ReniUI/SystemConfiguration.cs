using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;

namespace ReniUI
{
    static class
        SystemConfiguration
    {
        const string ConfigRoot = ".StudioConfig";

        internal static string UserSpecificConfigurationPath
            => OurFolder(Environment.SpecialFolder.ApplicationData);

        internal static string SystemConfigurationPath
            => OurFolder(Environment.SpecialFolder.CommonApplicationData);

        internal static FileConfiguration[] ActiveFileNames
        {
            get
            {
                var allKnownFileNames = AllKnownFileNames.ToArray();
                return allKnownFileNames
                    .Select(item => new FileConfiguration(item))
                    .Where(item => item.Status != "Closed")
                    .OrderBy(item => item.LastUsed)
                    .ToArray();
            }
        }

        static string EditorFilesPath => ConfigRoot.PathCombine("EditorFiles");

        static IEnumerable<string> AllKnownFileNames
            => ConfigurationPathsForAllKnownFiles
                .Select(GetEditorFileName);

        static IEnumerable<string> ConfigurationPathsForAllKnownFiles
        {
            get
            {
                var fileHandle = EditorFilesPath.ToSmbFile();
                if (fileHandle.Exists)
                    return fileHandle
                        .Items
                        .Select(item => item.FullName);

                return Enumerable.Empty<string>();
            }
        }

        static string OurFolder(Environment.SpecialFolder folder)
        {
            return OurFolder(Environment.GetFolderPath(folder));
        }

        internal static void QueryFileAndOpenIt
            (this IFileOpenController fileOpenController, IStudioApplication application)
        {
            fileOpenController.OnFileOpen("Reni file", "Reni files|*.reni|All files|*.*", "reni");
            if (fileOpenController.FileName == null)
                return;

            new EditorView(new FileConfiguration(fileOpenController.FileName), application)
                .Run();
        }

        internal static void OnFileOpen
        (
            this IFileOpenController controller,
            string title,
            string filter,
            string defaultExtension,
            int filterIndex = 2,
            bool checkFileExists = false,
            bool restoreDirectory = true
        )
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                RestoreDirectory = restoreDirectory,
                InitialDirectory = InitialDirectory(controller),
                FileName = controller.FileName?.ToSmbFile().Name,
                Filter = filter,
                CheckFileExists = checkFileExists,
                FilterIndex = filterIndex,
                DefaultExt = defaultExtension
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var newFile = dialog.FileName.ToSmbFile();
            if (!newFile.Exists)
                newFile.String = controller.CreateEmptyFile;
            controller.FileName = newFile.FullName;
        }


        internal static string GetConfigurationPath(string editorFileName)
        {
            var projectPath = ".".ToSmbFile().FullName + "\\";
            var fullFileName = editorFileName.ToSmbFile().FullName;

            Tracer.Assert(fullFileName.StartsWith(projectPath));
            var fileName = fullFileName.Substring(projectPath.Length);

            return GetKnownConfigurationPath(fileName, fullFileName)
                   ?? GetNewConfigurationPath(fileName);
        }

        static string GetKnownConfigurationPath(string fileName, string fullFileName)
        {
            EditorFilesPath.PathCombine(fileName).ToSmbFile().EnsureDirectoryOfFileExists();

            var result = ConfigurationPathsForAllKnownFiles
                .SingleOrDefault
                (item => GetEditorFileName(item).ToSmbFile().FullName == fullFileName);
            return result;
        }

        static string OurFolder(string head)
        {
            return head.PathCombine("HoyerWare", "ReniStudio");
        }

        static string GetNewConfigurationPath(string fileName)
        {
            var configurationFileName = fileName.Replace("\\", "_");

            while (true)
            {
                var duplicates = EditorFilesPath
                    .ToSmbFile()
                    .Items
                    .Select(item => item.Name)
                    .Count(item => item.StartsWith(configurationFileName));

                if (duplicates == 0)
                {
                    var result = EditorFilesPath.PathCombine(configurationFileName);
                    var nameFile = result.PathCombine("Name").ToSmbFile();
                    nameFile.String = fileName;
                    return result;
                }

                configurationFileName += "_" + duplicates;
            }
        }

        static string GetEditorFileName(string configurationPath)
        {
            return configurationPath.PathCombine("Name").ToSmbFile().String;
        }

        static string InitialDirectory(IFileOpenController controller)
        {
            return controller.FileName == null
                ? controller.DefaultDirectory.ToSmbFile().FullName
                : controller.FileName.ToSmbFile().DirectoryName;
        }
    }

    interface IFileOpenController
    {
        string FileName { get; set; }
        string CreateEmptyFile { get; }
        string DefaultDirectory { get; }
    }
}