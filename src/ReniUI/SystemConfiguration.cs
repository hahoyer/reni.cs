using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ReniUI
{
    sealed class SystemConfiguration
    {
        internal static string UserSpecificConfigurationPath
            => OurFolder(Environment.SpecialFolder.ApplicationData);

        internal static string SystemConfigurationPath
            => OurFolder(Environment.SpecialFolder.CommonApplicationData);

        static string OurFolder(Environment.SpecialFolder folder)
            => OurFolder(Environment.GetFolderPath(folder));

        static string OurFolder(string head) => Path.Combine(head, "HoyerWare", "ReniStudio");
        internal const string ConfigRoot = "StudioConfig";

        internal static string EditorFilesPath => Path.Combine(ConfigRoot, "EditorFiles");

        internal static string GetEditorConfigurationPath(string editorFileName)
            => Path.Combine(GetConfigurationPath(editorFileName), "EditorConfiguration");

        internal static string GetPositionPath(string editorFileName)
            => Path.Combine(GetConfigurationPath(editorFileName), "Position");

        internal static string GetConfigurationPath(string editorFileName)
        {
            var projectPath = ".".FileHandle().FullName + "\\";
            var fullFileName = editorFileName.FileHandle().FullName;

            Tracer.Assert(fullFileName.StartsWith(projectPath));
            var fileName = fullFileName.Substring(projectPath.Length);

            var result = ConfigurationPaths
                .SingleOrDefault(item => GetEditorFileName(item).FileHandle().FullName == fullFileName);

            return result ?? NewConfigurationPath(fileName);
        }

        static string NewConfigurationPath(string fileName)
        {
            var configurationFileName = fileName;

            while(true)
            {
                var duplicates = EditorFilesPath
                    .FileHandle()
                    .Items
                    .Select(item => item.Name)
                    .Count(item => item.StartsWith(configurationFileName));

                if (duplicates == 0)
                {
                    var result = Path.Combine(EditorFilesPath, configurationFileName);
                    var nameFile = Path.Combine(result, "Name").FileHandle();
                    nameFile.AssumeDirectoryOfFileExists();
                    nameFile.String = fileName;
                    return result;
                }

                configurationFileName += "_" + duplicates;
            } 
        }

        internal static IEnumerable<string> EditorFileNames
            => ConfigurationPaths.Select(GetEditorFileName);

        static IEnumerable<string> ConfigurationPaths
            => EditorFilesPath
                .FileHandle()
                .Items
                .Select(item => item.FullName);

        static string GetEditorFileName(string configurationPath)
            => Path.Combine(configurationPath, "Name").FileHandle().String;
    }
}