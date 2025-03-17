using System.ComponentModel.DataAnnotations;

namespace PhotoWebApp.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        public int PhotoId { get; set; }
        [Required]
        public string? commentValue {  get; set; }
        [Required]
        public DateTime DatePosted { get; set; } = DateTime.Now;
        public bool Flagged { get; set; }

        // related photo property
        public Photo? Photo { get; set; }
    }
}
