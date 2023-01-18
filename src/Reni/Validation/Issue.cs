using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Reni.Validation;

public sealed class Issue : DumpableObject, IEquatable<Issue>
{
    internal static readonly IEnumerable<Issue> Empty = new Issue[0];
    static int NextObjectId;

    [EnableDump]
    internal readonly SourcePart Position;

    [DisableDump]
    internal readonly IssueId IssueId;

    [EnableDump]
    readonly object[] AdditionalInformation;

    [DisableDump]
    internal string Tag => IssueId.Tag;

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

    internal Issue(IssueId issueId, SourcePart position, params object[] additionalInformation)
        : base(NextObjectId++)
    {
        IssueId = issueId;
        Position = position;
        AdditionalInformation = additionalInformation;
        AssertValid();
        StopByObjectIds();
    }

    bool IEquatable<Issue>.Equals(Issue other)
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

    public override bool Equals(object obj)
    {
        if(ReferenceEquals(null, obj))
            return false;
        if(ReferenceEquals(this, obj))
            return true;
        return obj is Issue issue && Equals(issue);
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

    void AssertValid() => (Position != null).Assert();

    public static bool operator ==(Issue left, Issue right) => Equals(left, right);
    public static bool operator !=(Issue left, Issue right) => !Equals(left, right);
}