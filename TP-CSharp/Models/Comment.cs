using System.ComponentModel.DataAnnotations;

namespace TP_CSharp.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public int BlogId { get; set; }
        public Blog? Blog { get; set; }

        public Guid? UserId { get; set; }

        public User? User { get; set; }
    }
}
