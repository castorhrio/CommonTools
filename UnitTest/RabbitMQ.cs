using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class RabbitMQ
    {
        private static string host = "localhost";
        private static int port = 5672;
        private static string user_name = "admin";
        private static string password = "mobstaz";

        private static string queue = "normal_queue";
        private static string exchange = "normal_exchange";
        private static string route = "normal_route";

        private static string delay_exchange = "delay_exchange";
        private static string delay_route = "delay_route";
        private static string delay_queue = "delay_queue";

        private static string dead_exchange = "dead_exchange";
        private static string dead_route = "dead_route";
        private static string dead_queue = "dead_queue";

        private static IConnection conn;
        private static IModel channel;

        private RabbitMQ()
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = host,
                Port = port,
                UserName = user_name,
                Password = password,
                RequestedHeartbeat = 60,
                AutomaticRecoveryEnabled = true
            };

            conn = factory.CreateConnection();
            channel = conn.CreateModel();

            DeclareDelay(channel);
            DeclareDead(channel);
        }

        private static class RabbitMQInstance
        {
            public static RabbitMQ _instance = new RabbitMQ();
        }

        public static RabbitMQ GetInstance()
        {
            return RabbitMQInstance._instance;
        }

        public void Sender(int data,string delay_time)
        {
            //奇数
            if ((data & 1) == 1)
            {
                channel.BasicPublish(dead_exchange, dead_route, null, Encoding.UTF8.GetBytes("this is dead"));
            }
            else
            {
                byte[] msg_byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                IBasicProperties properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;    //持久化消息
                properties.Expiration = delay_time; //消息过期时间
                channel.BasicPublish(delay_exchange, delay_route, properties, msg_byte);
            }
        }

        public void Receiver()
        {
            while (true)
            {
                using (var channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(exchange, "direct", true, false);
                    channel.QueueDeclare(queue, true, false, false, null);
                    channel.QueueBind(queue, exchange, route);

                    //回调，当consumer收到消息后会执行该函数
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var message = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine("Received {0}", message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    channel.BasicQos(0, 100, false);
                    channel.BasicConsume(queue: queue,
                                         autoAck: false,
                                         consumer: consumer);

                    Console.ReadLine();
                }
            }
        }

        private static void DeclareDelay(IModel channel)
        {
            channel.ExchangeDeclare(delay_exchange, "direct", true, false);
            Dictionary<string, object> arg = new Dictionary<string, object>();
            arg.Add("x-dead-letter-exchange", exchange);
            arg.Add("x-dead-letter-routing-key", route);
            channel.QueueDeclare(delay_queue, true, false, false, arg);
            channel.QueueBind(delay_queue, delay_exchange, delay_route);
        }

        private static void DeclareDead(IModel channel)
        {
            channel.ExchangeDeclare(dead_exchange, "direct", true, false);
            Dictionary<string, object> arg = new Dictionary<string, object>();
            arg.Add("x-dead-letter-exchange", exchange);
            arg.Add("x-dead-letter-routing-key", route);
            arg.Add("x-message-ttl", 5000);
            channel.QueueDeclare(dead_queue, true, false, false, arg);
            channel.QueueBind(dead_queue, dead_exchange, dead_route);
        }
    }
}
