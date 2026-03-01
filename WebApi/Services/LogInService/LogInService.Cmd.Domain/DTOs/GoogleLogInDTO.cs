using System.ComponentModel.DataAnnotations;

namespace LogInService.Cmd.Domain.DTOs
{
    public class GoogleLogInDTO
    {
        [Required]
        public string credential { get; set; }
    }
}

