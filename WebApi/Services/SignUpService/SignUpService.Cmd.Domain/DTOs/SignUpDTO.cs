using System.ComponentModel.DataAnnotations;

namespace SignUpService.Cmd.Domain.DTOs
{
    public class SignUpDTO
    {
        [Required]
        public string userId { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public string name { get; set; }
    }
}

