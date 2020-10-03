namespace Reni.Parser
{
    interface ISyntaxScope
    {
        IDefaultScopeProvider DefaultScopeProvider { get; }
        bool IsDeclarationPart { get; }
    }
}