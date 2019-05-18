using RabbitMQ.Client;

namespace CommonTools.RabbitMQTool
{
    public class RabbitMQClientContext
    {
        public string Id { get; set; }

        public IConnection SendConnection { get; set; }

        public IModel SendChannel { get; set; }

        public IConnection ReceiveConnection { get; set; }

        public IModel ReceiveChannel { get; set; }

        public string QueueName { get; set; }
    }
}