using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.RabbitMQTool
{
    public class RabbitMQConfig
    {
        public string Host { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string QueueName { get; set; }
    }
}
