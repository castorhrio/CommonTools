using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.RabbitMQTool
{
    public class RabbitMQClientContext
    {
        public string Id { get; set; }

        public IConnection SendConnection { get; set; }

        public IModel SendChannel { get; set; }

        public IConnection ReceiveConnection { get; set; }

        public IModel ReceiveChannel { get; set; }

        public string ReceiveQueueName { get; set; }
    }
}
