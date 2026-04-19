using System;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Comment
{
    public class CreateCommentRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(2000)]
        public string Content { get; set; } = null!;
    }

    public class UpdateCommentRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(2000)]
        public string Content { get; set; } = null!;
    }

    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Vote { get; set; }
        public bool IsAnswer { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class CommentDetailDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Vote { get; set; }
        public bool IsAnswer { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public bool IsLikedByUser { get; set; }
    }
}
