using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Background.Models
{
    public class DbConnectionStatus
    {
        public int Active { get; set; }
        public int Idle { get; set; }
        public int Total { get; set; }
        public int Idle_in_txn { get; set; }
    }
}

