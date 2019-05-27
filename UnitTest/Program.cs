using CommonTools.RabbitMQTool;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("start");
            //for(int i = 0; i < 100; i++)
            //{
            //    Task.Factory.StartNew(() => SendMessage());
            //    Task.Factory.StartNew(() => DelayReceive());
            //}
            Task.Factory.StartNew(() => SendMessage());
            ////Task.Factory.StartNew(() => ReceiveMessage());
            Task.Factory.StartNew(() => DelayReceive());
            //Task.Factory.StartNew(Sender);
            //Task.Factory.StartNew(Receive);
            Console.ReadKey();
            Console.WriteLine("end");
        }

        private static void SendMessage()
        {
            for (int i = 0; i < 1000; i++)
            {
                RabbitData data = new RabbitData
                {
                    id = i,
                    time = DateTime.Now,
                    data = "this is data :" + i
                };

                var send = MessageFactory.CreateMessageInstance(data);
                //RabbitMQClient.Instance.Sender(send);
                RabbitMQClient.Instance.DelaySender(send, 3000);
            }
        }

        private static void ReceiveMessage()
        {
            RabbitMQClient.Instance.ActionEvent += EventMessage;
            RabbitMQClient.Instance.OnListening();
        }

        private static void DelayReceive()
        {
            RabbitMQClient.Instance.ActionEvent += DelayEventMessage;
            RabbitMQClient.Instance.DelayListening();
        }

        private static void EventMessage(MessageResult result)
        {
            var message = JsonConvert.DeserializeObject<RabbitData>(result.Messages);
            Console.WriteLine("handle success !" + message.data);
        }

        private static void DelayEventMessage(MessageResult result)
        {
            var message = JsonConvert.DeserializeObject<RabbitData>(result.Messages);
            Console.WriteLine("delay handle success !" + message.data);
        }

        private static void SendDelayMsg()
        {
            //首先创建一个连接工厂对象
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "mobstaz", Port = 5672 };
            //然后使用工厂对象创建一个TCP连接
            using (var connection = factory.CreateConnection())
            {
                //在当前连接上创建一根通信的虚拟管道
                using (var channel = connection.CreateModel())
                {
                    //声明一个交换机
                    channel.ExchangeDeclare("e.log", "direct");
                    //声明一个队列，设置arguments的参数x-message-ttl为10000毫秒
                    channel.QueueDeclare(queue: "q.log.error",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: new Dictionary<string, object> {
                                         { "x-message-ttl",10000} //x-message-ttl即设置当前队列消息的过期时间。ttl即为time to live
                                         });

                    channel.QueueBind("q.log.error", //队列名称
                                      "e.log",      //交换机名称
                                      "log.error");  //自定义的RoutingKey

                    var body = Encoding.UTF8.GetBytes("测试消息");
                    var properties = channel.CreateBasicProperties();
                    //设置消息持久化
                    properties.DeliveryMode = 2;

                    //发布消息
                    channel.BasicPublish(exchange: "e.log",
                                         routingKey: "log.error",
                                         basicProperties: properties,
                                         body: body);
                }
            }
        }

        private static void SendDelayOnMsg()
        {
            //首先创建一个连接工厂对象
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "mobstaz", Port = 5672 };
            //然后使用工厂对象创建一个TCP连接
            using (var connection = factory.CreateConnection())
            {
                //在当前连接上创建一根通信的虚拟管道
                using (var channel = connection.CreateModel())
                {
                    //声明一个交换机
                    channel.ExchangeDeclare("e.log", "direct");
                    //声明一个队列，设置arguments的参数x-message-ttl为10000毫秒
                    channel.QueueDeclare(queue: "q.log.error",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.QueueBind("q.log.error", //队列名称
                                      "e.log",      //交换机名称
                                      "log.error");  //自定义的RoutingKey

                    var body = Encoding.UTF8.GetBytes("测试消息");
                    var properties = channel.CreateBasicProperties();
                    //设置消息持久化
                    properties.DeliveryMode = 2;

                    //设置当个消息的过期时间为5000毫秒
                    properties.Expiration = "5000";

                    channel.BasicPublish(exchange: "e.log",
                                         routingKey: "log.error",
                                         basicProperties: properties,
                                         body: body);
                }
            }
        }

        private static void SendMsg()
        {
            //首先创建一个连接工厂对象
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "mobstaz", Port = 5672 };
            //然后使用工厂对象创建一个TCP连接
            using (var connection = factory.CreateConnection())
            {
                //在当前连接上创建一根通信的虚拟管道
                using (var channel = connection.CreateModel())
                {
                    //声明一个交换机
                    channel.ExchangeDeclare("e.log", "direct");
                    //声明一个队列，设置arguments的参数x-message-ttl为10000毫秒
                    channel.QueueDeclare(queue: "q.log.error",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.QueueBind("q.log.error", //队列名称
                                      "e.log",      //交换机名称
                                      "log.error");  //自定义的RoutingKey

                    var body = Encoding.UTF8.GetBytes("测试消息");
                    var properties = channel.CreateBasicProperties();
                    //设置消息持久化
                    properties.DeliveryMode = 2;

                    //发布消息
                    channel.BasicPublish(exchange: "e.log",
                                         routingKey: "log.error",
                                         basicProperties: properties,
                                         body: body);
                }
            }
        }

        private static void Sender()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "mobstaz", Port = 5672 };

            using (var connection = factory.CreateConnection())
            {
                while (Console.ReadLine() != null)
                {
                    using (var channel = connection.CreateModel())
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        //dic.Add("x-expires", 30000);
                        dic.Add("x-message-ttl", 10000);//队列上消息过期时间，应小于队列过期时间
                        dic.Add("x-dead-letter-exchange", "exchange-direct");//过期消息转向路由
                        dic.Add("x-dead-letter-routing-key", "routing-delay");//过期消息转向路由相匹配routingkey
                        //创建一个名叫"zzhello"的消息队列
                        channel.QueueDeclare(queue: "zzhello",
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: dic);

                        var message = "Hello World!";
                        var body = Encoding.UTF8.GetBytes(message);

                        //向该消息队列发送消息message
                        channel.BasicPublish(exchange: "",
                            routingKey: "zzhello",
                            basicProperties: null,
                            body: body);
                        Console.WriteLine(" [x] Sent {0}", message);
                    }
                }
            }
        }

        private static void Receive()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "mobstaz", Port = 5672 };

            while (true)
            {
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "exchange-direct", type: "direct");
                        string name = channel.QueueDeclare().QueueName;
                        channel.QueueBind(queue: name, exchange: "exchange-direct", routingKey: "routing-delay");

                        //回调，当consumer收到消息后会执行该函数
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);
                            Console.WriteLine(ea.RoutingKey);
                            Console.WriteLine(" [x] Received {0}", message);
                        };

                        //Console.WriteLine("name:" + name);
                        //消费队列"hello"中的消息
                        channel.BasicConsume(queue: name,
                                             autoAck: true,
                                             consumer: consumer);

                        Console.ReadLine();
                    }
                }
            }
        }

        private class RabbitData
        {
            public int id { get; set; }

            public DateTime time { get; set; }

            public string data { get; set; }
        }
    }
}