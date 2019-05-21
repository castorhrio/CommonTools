using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace CommonTools.RabbitMQTool
{
    public class RabbitMQClientContext
    {
        public IConnection SendConnection { get; set; }

        public IModel SendChannel { get; set; }

        public IConnection ReceiveConnection { get; set; }

        public IModel ReceiveChannel { get; set; }
    }

    public class RabbitMQClientFactory
    {
        private const ushort heartbeat = 60;

        /// <summary>
        ///  创建一个单例的RabbitMQClient实例
        /// </summary>
        /// <returns></returns>
        public static IRabbitMQClient GetClientInstance()
        {
            RabbitMQConfig config = GetRabbitMQConfig();
            RabbitMQClient.Instance = new RabbitMQClient()
            {
                Config = config,
                Context = new RabbitMQClientContext()
            };

            return RabbitMQClient.Instance;
        }

        /// <summary>
        /// 创建一个IConnection
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IConnection CreateConnection(RabbitMQConfig config)
        {
            if (config == null)
                throw new Exception("Rabbit Config Error");

            ConnectionFactory factory = new ConnectionFactory()
            {
                UserName = config.UserName,
                Password = config.Password,
                HostName = config.Host,
                Port = config.Port,
                RequestedHeartbeat = heartbeat,
                AutomaticRecoveryEnabled = true
            };

            return factory.CreateConnection();
        }

        /// <summary>
        /// 创建一个IModel
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static IModel CreateChannel(IConnection conn)
        {
            return conn.CreateModel();
        }

        public static void ExchangeDeclare(IModel channel, string exchangeName, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments=null)
        {
            channel.ExchangeDeclare(exchangeName, type, durable, autoDelete, arguments);
        }

        public static void QueueDeclare(IModel channel, string queueName, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments=null)
        {
            channel.QueueDeclare(queueName, durable, exclusive, autoDelete, arguments);
        }

        public static void QueueBind(IModel channel, string queue, string exchange, string routingKey, IDictionary<string, object> arguments=null)
        {
            channel.QueueBind(queue, exchange, routingKey, arguments);
        }

        /// <summary>
        /// 获取RabbitMQ配置
        /// </summary>
        /// <returns></returns>
        public static RabbitMQConfig GetRabbitMQConfig()
        {
            RabbitMQConfig config = null;
            string host = ConfigurationManager.AppSettings["RabbitMQ_Host"];
            int port = int.Parse(ConfigurationManager.AppSettings["RabbitMQ_Port"]);
            string user_name = ConfigurationManager.AppSettings["RabbitMQ_UserName"];
            string password = ConfigurationManager.AppSettings["RabbitMQ_Password"];
            string queue_name = ConfigurationManager.AppSettings["RabbitMQ_Queue"];
            string exchange_name = ConfigurationManager.AppSettings["RabbitMQ_Exchange"];
            string route_key = ConfigurationManager.AppSettings["RabbitMQ_RouteKey"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user_name) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(queue_name))
                throw new ArgumentNullException("RabbitMQ Config Error");

            config = new RabbitMQConfig
            {
                Host = host,
                Port = port,
                UserName = user_name,
                Password = password,
                QueueName = queue_name,
                ExchangeName = exchange_name,
                RouteKey = route_key
            };

            return config;
        }
    }
}