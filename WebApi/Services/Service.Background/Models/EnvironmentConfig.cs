using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Background.Models
{
    public class EnvironmentConfig
    {
        public string EnvironmentName { get; set; }
        public string DiscordWebHookUrl { get; set; }
        public float ConnectionWarningThreshold { get; set; } = 0.8f; //?êË®≠Ë≠¶Â??æÂÄ?0%
    }
}

