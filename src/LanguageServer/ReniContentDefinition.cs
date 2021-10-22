using System.ComponentModel.Composition;
using JetBrains.Annotations;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace ReniExtension
{
    [UsedImplicitly]
    public class ReniContentDefinition
    {
        [Export]
        [Name("reni")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        [UsedImplicitly]
        internal static ContentTypeDefinition ReniContentTypeDefinition;

        [Export]
        [FileExtension(".reni")]
        [ContentType("reni")]
        [UsedImplicitly]
        internal static FileExtensionToContentTypeDefinition ReniFileExtensionDefinition;
    }
}