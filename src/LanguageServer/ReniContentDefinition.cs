using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace ReniExtension
{
    public class ReniContentDefinition
    {
        [Export]
        [Name("reni")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition ReniContentTypeDefinition;

        [Export]
        [FileExtension(".reni")]
        [ContentType("reni")]
        internal static FileExtensionToContentTypeDefinition ReniFileExtensionDefinition;
    }
}