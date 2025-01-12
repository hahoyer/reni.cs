namespace Reni.FeatureTest.Helper
{
    static class Extension
    {
        public static void AssertBinaryTreeIsLike(this Compiler compiler, LikeSyntax prototype)
            => prototype.AssertLike(compiler.BinaryTree.Left!.Right);

        public static void AssertSyntaxIsLike(this Compiler compiler, LikeSyntax prototype)
            => prototype.AssertLike(compiler.Syntax);
    }
}