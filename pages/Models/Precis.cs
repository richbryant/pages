using System;

namespace pages.Models
{
    public record Precis
    {
        public string Thumbnail { get; init; }
        public string Title { get; init; }
        public Metadata Metadata { get; init; }
        public string Snippet { get; init; }
        public string Link { get; init; }
    }

    public record Metadata
    {
        public string Date { get; init; }
        public string Time { get; init; }
    }
}