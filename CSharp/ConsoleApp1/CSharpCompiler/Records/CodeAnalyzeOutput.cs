namespace General.Automation.Subscriptions.WebHooks.CSharpCompiler.Records
{
    public record CodeAnalyzeOutput
    {
        public bool Success { get; init; }
        public string Errors { get; init; }
        public string Result { get; set; }
    }
}
