using RabbitMQ.Client;
using System;

namespace CommonTools
{
    public class RabbitMQHelper
    {
        public class RabbitMQConfigModel
        {
            public string host_name { get; set; }
            public int port { get; set; }
            public string user_name { get; set; }
            public string pass_word { get; set; }
            public ushort? max_channel_count { get; set; }

            public bool _auto_recovery_ = true;
        }

        private static ushort _default_channel_count = 100;
        private static Lazy<IConnection> _factory_multiplexer;
        private static IConnection _connection => _factory_multiplexer.Value;
        private static IModel _channel => _connection.CreateModel();

        private const int _subdivision_count = 10;    //队列分组
        private static ushort batch_size = 100;    //QPS

        private RabbitMQHelper()
        {
        }

        public static RabbitMQHelper GetInstance(RabbitMQConfigModel mq_config)
        {
            if (mq_config == null)
                return null;

            _factory_multiplexer = new Lazy<IConnection>(() =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = mq_config.host_name,
                    Port = mq_config.port,
                    UserName = mq_config.user_name,
                    Password = mq_config.pass_word,
                    RequestedChannelMax = mq_config.max_channel_count.HasValue ? mq_config.max_channel_count.Value : _default_channel_count,
                    AutomaticRecoveryEnabled = true
                };

                return factory.CreateConnection();
            });

            return new RabbitMQHelper();
        }
    }
}