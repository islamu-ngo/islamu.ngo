using System.ComponentModel.DataAnnotations;

namespace iLoveIbadah.Website.Models
{
    public class CommentVM
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int UserAccountId { get; set; }
        public string Content { get; set; }
        public DateTime? WrittenAt { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentVM>? Replies { get; set; }
    }

    public class CommentListVM
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int UserAccountId { get; set; }
        public string Content { get; set; }
        public DateTime? WrittenAt { get; set; }
        public int? ParentCommentId { get; set; }
    }

    public class CreateCommentVM
    {
        [Required]
        public int BlogId { get; set; }
        public int? UserAccountId { get; set; }
        [Required]
        public string Content { get; set; }
        public int? ParentCommentId { get; set; }
    }

    public class UpdateCommentVM
    {
        [Required]
        public int Id { get; set; }
        public int? UserAccountId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}