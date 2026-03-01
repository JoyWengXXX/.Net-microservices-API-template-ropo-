using System.ComponentModel.DataAnnotations;

namespace ControllerService.Cmd.Domain.DTOs
{
    public class AddControllerDTO
    {
        [Required]
        public string controllerId { get; set; }
        [Required]
        public string controllerName { get; set; }
    }
}

