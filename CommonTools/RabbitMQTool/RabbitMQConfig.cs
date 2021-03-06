﻿namespace CommonTools.RabbitMQTool
{
    public class RabbitMQConfig
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string QueueName { get; set; }

        public string ExchangeName { get; set; }

        public string RouteKey { get; set; }
    }
}