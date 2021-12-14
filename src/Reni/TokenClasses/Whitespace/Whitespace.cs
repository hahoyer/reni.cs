namespace Reni.TokenClasses.Whitespace
{
    interface ISpace : IItemType { }

    interface IComment : IItemType { }

    interface ILineBreak : IItemType { }

    interface IStable : IItemType { }

    interface IStableLineBreak : IStable, ILineBreak { }

    interface IVolatileLineBreak : ILineBreak { }

    interface IParent
    {
        IWhitespaceItem GetItem<TItemType>()
            where TItemType : IItemType;

        IItemType Type { get; }
        IParent Parent { get; }
    }

    namespace Comment
    {
        interface ILine : IComment { }

        interface IInline : IComment { }

        namespace Line
        {
            interface IHead : IStable { }

            interface IText : IStable { }

            interface ITail : IStableLineBreak { }
        }

        namespace Inline
        {
            interface IHead : IStable { }

            interface IIdentifier : IStable { }

            interface IText : IItemType { }

            interface ITail : IStable { }

            namespace Text
            {
                interface IText : IStable { }

                interface ILineBreak : IStableLineBreak { }
            }
        }
    }
}