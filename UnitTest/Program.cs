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

            var send = MessageFactory.CreateMessageInstance(data);
            RabbitMQClient.Instance.EventTrigger(send, 5000);
        }

        static void ReceiveMessage()
        {
            RabbitMQClient.Instance.ActionEvent += EventMessage;
            RabbitMQClient.Instance.OnListening(true);
        }

        private static void EventMessage(MessageResult result)
        {
            var message = JsonConvert.DeserializeObject<RabbitData>(result.Messages);
            Console.WriteLine("handle success !" + message.data);
        }

        class RabbitData
        {
            public int id { get; set; }

            public DateTime time { get; set; }

            public string data { get; set; }
        }
    }
}
