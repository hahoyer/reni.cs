namespace Reni.TokenClasses {
    interface IBelongingsMatcher
    {
        bool IsBelongingTo(IBelongingsMatcher otherMatcher);
    }

}