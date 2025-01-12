using System.ComponentModel.Composition;
using JetBrains.Annotations;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX;

public class ReniContent
{
    [UsedImplicitly]
    [Export]
    [Name(Constants.LanguageName)]
    [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteBaseTypeName)]
    public static ContentTypeDefinition ContentType;

    [UsedImplicitly]
    [Export]
    [FileExtension(".reni")]
    [ContentType(Constants.LanguageName)]
    public static FileExtensionToContentTypeDefinition FileExtension;
}