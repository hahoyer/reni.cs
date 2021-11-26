using System.ComponentModel.Composition;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX
{
    public static class ReniContent
    {
        [UsedImplicitly]
        [Export]
        [Name("reni")]
        [BaseDefinition("code")]
        public static ContentTypeDefinition ContentType;

        [UsedImplicitly]
        [Export]
        [FileExtension(".reni")]
        [ContentType("reni")]
        public static FileExtensionToContentTypeDefinition FileExtension;
    }
}