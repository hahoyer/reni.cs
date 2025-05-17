using hw.Scanner;

using Reni.Context;

namespace Reni.Validation;

public sealed class Issue : DumpableObject, IEquatable<Issue>
{
    internal static readonly IEnumerable<Issue> Empty = new Issue[0];
    static int NextObjectId;

    [EnableDump]
    internal readonly SourcePart Position;

    [DisableDump]
    internal readonly IssueId IssueId;

    [DisableDump]
    internal readonly Root Root;

    [EnableDump]
    internal readonly object[] AdditionalInformation;

    [DisableDump]
    internal string Tag => $"{IssueId}";

    internal string LogDump
    {
        get
        {
            var result = Position.Id == "("
                ? Position.Start.FilePosition(Tag) + " Functional"
                : Position.FileErrorPosition(Tag);
            var additionalInformation = AdditionalInformation.Select(p => " " + p.NodeDump()).Stringify("");
            return result + additionalInformation;
        }
    }

    [DisableDump]
    internal string Message => IssueId.GetMessage(AdditionalInformation);

    internal Issue(IssueId issueId, Root? root, SourcePart position,  params object[] additionalInformation)
        : base(NextObjectId++)
    {
        IssueId = issueId;
        Root = root.AssertNotNull();
        Position = position;
        AdditionalInformation = additionalInformation;
        StopByObjectIds();
    }

    bool IEquatable<Issue>.Equals(Issue? other)
    {
        if(ReferenceEquals(null, other))
            return false;
        if(ReferenceEquals(this, other))
            return true;
        return
            Equals(Position, other.Position)
            && Equals(IssueId, other.IssueId)
            && AdditionalInformation.SequenceEqual(other.AdditionalInformation);
    }

    protected override string GetNodeDump()
        => base.GetNodeDump() + IssueId.NodeDump().Surround("{", "}");

    public override bool Equals(object? obj)
    {
        if(ReferenceEquals(null, obj))
            return false;
        if(ReferenceEquals(this, obj))
            return true;
        return obj is IEquatable<Issue> issue && issue.Equals(this);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Position.GetHashCode();
            hashCode = (hashCode * 397) ^ IssueId.GetHashCode();
            hashCode = (hashCode * 397) ^ AdditionalInformation.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Issue? left, Issue? right) => Equals(left, right);
    public static bool operator !=(Issue? left, Issue? right) => !Equals(left, right);
}