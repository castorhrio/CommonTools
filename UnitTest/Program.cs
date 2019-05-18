using CommonTools.RabbitMQTool;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start");
            //SendMessage();
            Task.Factory.StartNew(() => SendMessage());
            Task.Factory.StartNew(() => ReceiveMessage());
            Console.ReadKey();
            Console.WriteLine("end");
        }

        static void SendMessage()
        {
            //string queue_name = ConfigurationManager.AppSettings["RabbitMQ_Queue"];
            //string exchange_name = ConfigurationManager.AppSettings["RabbitMQ_Exchange"];
            //string host = ConfigurationManager.AppSettings["RabbitMQ_Host"];
            //int port = int.Parse(ConfigurationManager.AppSettings["RabbitMQ_Port"]);
            //string user_name = ConfigurationManager.AppSettings["RabbitMQ_UserName"];
            //string password = ConfigurationManager.AppSettings["RabbitMQ_Password"];

            //ConnectionFactory factory = new ConnectionFactory();
            //factory.HostName = host;
            //factory.Port = port;
            //factory.UserName = user_name;
            //factory.Password = password;

            //using (IConnection conn = factory.CreateConnection())
            //{
            //    using (IModel channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(exchange_name, ExchangeType.Direct, true, false,null);
            //        channel.QueueDeclare(queue_name, true, false, false, null);


            //    }
            //}

            for (var i = 0; i < int.MaxValue; i++)
            {
                RabbitData data = new RabbitData
                {
                    id = i,
                    time = DateTime.Now,
                    data = "this is data :" + i
                };

                var send = EventMessageFactory.CreateEventMessageInstance(data, Guid.NewGuid().ToString("N"));
                RabbitMQClient.Instance.EventTrigger(send);

                Console.WriteLine("send message:" + i);
            }

            Console.Write("total count:" + int.MaxValue);
        }

        static void ReceiveMessage()
        {
            RabbitMQClient.Instance.ActionEvent += EventMessage;
            RabbitMQClient.Instance.OnListening();
        }


        private static void EventMessage(EventMessageResult result)
        {
            var message = JsonConvert.DeserializeObject<RabbitData>(Encoding.UTF8.GetString(result.Messages));
            if (result.Status)
            {
                Console.WriteLine("handle success !" + message.data);
            }
        }

        class RabbitData
        {
            public int id { get; set; }

            public DateTime time { get; set; }

            public string data { get; set; }
        }
    }
}
