using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser;

sealed class MainTokenFactory : GenericTokenFactory<BinaryTree>
{
    readonly Compiler<BinaryTree>.Component Declaration;
    readonly List<UserSymbol> UserSymbols = new();

    [DisableDump]
    internal IEnumerable<IParserTokenType<BinaryTree>> AllTokenClasses
        => PredefinedTokenClasses.Concat(UserSymbols);

    public MainTokenFactory(Compiler<BinaryTree>.Component declaration, string title = "Main")
        : base(title)
        => Declaration = declaration;

    protected override IParserTokenType<BinaryTree> SpecialTokenClass(System.Type type)
    {
        if(type == typeof(Exclamation))
            return new Exclamation(Declaration.SubParser);

        return base.SpecialTokenClass(type);
    }

    protected override IParserTokenType<BinaryTree> GetTokenClass(string name)
    {
        var result = new UserSymbol(name);
        UserSymbols.Add(result);
        return result;
    }
}