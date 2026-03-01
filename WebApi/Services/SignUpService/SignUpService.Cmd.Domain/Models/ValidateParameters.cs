using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignUpService.Cmd.Domain.Models
{
    public class ValidateParameters
    {
        public string userId { get; set; }
        public string validationCode { get; set; }
    }
}

