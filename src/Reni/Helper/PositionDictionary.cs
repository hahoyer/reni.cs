using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Helper;

public sealed class PositionDictionary<TResult> : DumpableObject
    where TResult : ITree<TResult>
{
    static int Position;

    readonly Dictionary<SourcePosition, TResult> InvisibleValues = new();
    readonly Dictionary<SourcePosition, TResult> VisibleValues = new();

    [DisableDump]
    internal TResult this[SourcePosition key] => VisibleValues[key];

    [DisableDump]
    TResult? this[SourcePart keyPart]
    {
        get => GetItem(keyPart);
        set => Add(keyPart.Length == 0? InvisibleValues : VisibleValues, keyPart.Start, value);
    }

    [DisableDump]
    internal TResult? this[BinaryTree key]
    {
        get => this[KeyMap(key)];
        set => this[KeyMap(key)] = value;
    }

    TResult? GetItem(SourcePart keyPart)
    {
        (keyPart.Length == 0? InvisibleValues : VisibleValues).TryGetValue(keyPart.Start, out var result);
        return result;
    }

    static void Add(Dictionary<SourcePosition, TResult> values, SourcePosition key, TResult value)
    {
        values.TryGetValue(key, out var oldValue);
        if(ReferenceEquals(oldValue, value))
            return;
        oldValue
            .AssertIsNull
            (
                ()
                    => $@"Tree-item is anchor in more than one {typeof(TResult).PrettyName()}-item: 
key = {key.DebuggerDump()}
value = {value.LogDump()}
oldValue = {oldValue!.LogDump()}"
            );
        values[key] = value;
    }

    static SourcePart KeyMap(BinaryTree key) => key.FullToken;

    public void AssertValid(BinaryTree binary)
    {
        lock(this)
        {
            Position = 0;
            binary
                .GetNodesFromLeftToRight()
                .Select(IsValidKey)
                .ToArray()
                .All(i => i)
                .Assert(()
                    => $@"Former tree-items not appearing in any {typeof(TResult).PrettyName()}-item as anchor.");
        }
    }

    bool IsValidKey(BinaryTree keyItem)
    {
        var value = this[keyItem];
        if(value != null)
            return true;
        @$" 
 -----
/{Position++:D5}\    BinaryTree.ObjectId={keyItem.ObjectId}
key: -----------------------------------------vv
{keyItem.Token.GetDumpAroundCurrent(20)}
----------------------------------------------^^".Log();

        return false;
    }
}