namespace CrewBackend.Helpers
{
    public class PaginationLink
    {
        public string? Url { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}