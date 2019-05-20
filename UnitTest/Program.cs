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
            RabbitData data = new RabbitData
            {
                id = 1,
                time = DateTime.Now,
                data = "this is data :" + 1
            };

            var send = EventMessageFactory.CreateEventMessageInstance(data);
            RabbitMQClient.Instance.EventTrigger(send);
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
