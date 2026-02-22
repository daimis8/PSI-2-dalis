using System.ComponentModel.DataAnnotations;

namespace PSI.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } = default!;

        [Required]
        public byte[] PasswordHash { get; set; } = default!;

        [Required]
        public byte[] PasswordSalt { get; set; } = default!;
    }
}
