﻿namespace CSharp
{
    public record CodeAnalyzeOutput
    {
        public bool Success { get; init; }
        public string? Errors { get; init; }

        public string? Result { get; set; }
    }
}
