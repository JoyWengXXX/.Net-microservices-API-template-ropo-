using System.ComponentModel.DataAnnotations;

namespace SignUpService.Cmd.Domain.DTOs
{
    public class ForgetPasswordDTO
    {
        [Required]
        public string userId { get; set; }
    }
}

