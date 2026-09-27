using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events
{
    public class PaymentProcessedEvent
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public string Status { get; set; }
    }
}
