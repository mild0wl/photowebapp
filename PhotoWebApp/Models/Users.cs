using System.ComponentModel.DataAnnotations;

namespace PhotoWebApp.Models
{
    public class Users
    {
        public int UserId { get; set; }
        [Required]
        [StringLength(15)]
        public string? Username { get; set; }
        [Required,StringLength(80)]
        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiry { get; set; }
        [Required]
        public UserRole Role { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();

        public static implicit operator Users(List<Users> v)
        {
            throw new NotImplementedException();
        }
    }

    public enum UserRole
    {
        Unknown,
        Admin = 1,
        ContentCreator = 2
    }
}
