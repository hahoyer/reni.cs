namespace ReniUI.RestFul
{
    public sealed class Issue
    {
        internal static Issue Create(Reni.Validation.Issue arg)
            => new Issue
            {
                Source= arg.Position.Source.Identifier,
                Start = arg.Position.Position,
                Length = arg.Position.Length,
                Id = $"{arg.IssueId}",
                Message = arg.Message
            };

        public string Source;
        public int Start;
        public int Length;
        public string Id;
        public string Message;
    }
}