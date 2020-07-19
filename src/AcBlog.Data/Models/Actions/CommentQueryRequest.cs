﻿namespace AcBlog.Data.Models.Actions
{
    public class CommentQueryRequest : QueryRequest
    {
        public string Id { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public string Uri { get; set; } = string.Empty;
    }
}
