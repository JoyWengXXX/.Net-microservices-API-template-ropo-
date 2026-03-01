using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInService.Cmd.Domain.Models
{
    public class GoogleCredentialPayload
    {
        public string email { get; set; }
        public string name { get; set; }
        public string photo { get; set; }
    }
}

