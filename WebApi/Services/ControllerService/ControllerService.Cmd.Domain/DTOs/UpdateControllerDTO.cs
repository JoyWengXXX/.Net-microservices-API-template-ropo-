using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerService.Cmd.Domain.DTOs
{
    public class UpdateControllerDTO
    {
        [Required]
        public string controllerId { get; set; }

        [Required]
        public string controllerName { get; set; }
    }
}

