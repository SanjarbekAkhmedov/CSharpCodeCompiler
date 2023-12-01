namespace CSharp
{
    public record DotNetVersionInfo
    {
        public string? Version { get; set; }

        public ICollection<string> AvailableMetaData { get; set; } = new List<string>();

        public ICollection<string> DefaultUsing { get; set; } = new List<string>();

        public string? Language { get; set; }
    }
}
