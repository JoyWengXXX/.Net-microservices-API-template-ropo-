using System.ComponentModel.DataAnnotations;

namespace LogInService.Cmd.Domain.DTOs
{
    public class EnableNotificationDTO
    {

        [Required]
        public int messageType { get; set; }

        [Required]
        public bool enable { get; set; }

        public string? subscription { get; set; }
    }
}

