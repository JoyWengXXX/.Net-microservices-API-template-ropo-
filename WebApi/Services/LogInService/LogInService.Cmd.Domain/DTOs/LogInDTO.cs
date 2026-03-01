using System.ComponentModel.DataAnnotations;

namespace LogInService.Cmd.Domain.DTOs
{
    public class LogInDTO
    {
        [Required]
        public string userId { get; set; }
        [Required]
        public string password { get; set; }
    }
}

