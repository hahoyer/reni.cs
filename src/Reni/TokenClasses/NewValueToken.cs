using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.Feature;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class NewValueToken : NonSuffixSyntaxToken
{
    public const string TokenId = "new_value";
    static readonly Declaration[] PredefinedDeclarations = { new("dumpprint") };

    protected override Declaration[] Declarations => PredefinedDeclarations;
    public override string Id => TokenId;

    protected override Result GetResult(ContextBase context, Category category)
        => context
            .FindRecentFunctionContextObject
            .CreateValueReferenceResult(category);

    protected override(Result, IImplementation) GetResult
        (ContextBase context, Category category, ValueSyntax right, SourcePart token)
    {
        NotImplementedMethod(context, category, token, right);
        return default;
    }
}