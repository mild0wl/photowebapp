using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoWebApp.Models
{
    public class Photo
    {
        [Key]
        public int photoId { get; set; }
        public required string photoTitle { get; set; }
        public string description { get; set; }
        [NotMapped]
        public IFormFile PhotoFile { get; set; }
        public string? FilePath { get; set; }
        public DateTime DatePosted { get; set; }
        public int userId { get; set; }
        public string tags { get; set; }
        [Required]
        public bool CommentMode { get; set; }
        public int LikesCount { get; set; }
        [Required]
        public bool IsPublic { get; set; }

        public virtual Users? User { get; set; }
        // comment collection
        public ICollection<Comment>? Comment { get; set; } = new List<Comment>();
    }
}
