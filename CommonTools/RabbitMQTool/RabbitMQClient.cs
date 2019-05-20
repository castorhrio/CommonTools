using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.RabbitMQTool
{
    public delegate void ActionEvent(MessageResult result);

    public class RabbitMQClient : IRabbitMQClient
    {
        public RabbitMQClientContext Context { get; set; }
        public RabbitMQConfig Config { get; set; }

        private static IRabbitMQClient _client;

        public static IRabbitMQClient Instance
        {
            get
            {
                if (_client == null)
                    RabbitMQClientFactory.GetClientInstance();

                return _client;
            }

            internal set { _client = value; }
        }

        private ActionEvent _action_message;

        public event ActionEvent ActionEvent
        {
            add
            {
                if (_action_message == null)
                    _action_message += value;
            }

            remove
            {
                if (_action_message != null)
                    _action_message -= value;
            }
        }

        public void EventTrigger(MessageData message,int? delay)
        {
            Context.SendConnection = RabbitMQClientFactory.CreateConnection(Config);
            using (Context.SendConnection)
            {
                Context.SendChannel = RabbitMQClientFactory.CreateChannel(Context.SendConnection);
                using (Context.SendChannel)
                {
                    Dictionary<string, object> dic = null;
                    if (delay != null)
                    {
                        dic = new Dictionary<string, object>();
                        dic.Add("x-message-ttl", delay.Value);//队列上消息过期时间，应小于队列过期时间  
                        dic.Add("x-dead-letter-exchange", "exchange-direct");//过期消息转向路由  
                        dic.Add("x-dead-letter-routing-key", "routing-delay");//过期消息转向路由相匹配routingkey
                    }

                    RabbitMQClientFactory.QueueDeclare(Context.SendChannel, Config.QueueName, true, false, false, dic);
                    byte[] msg_byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    IBasicProperties properties = Context.SendChannel.CreateBasicProperties();
                    properties.DeliveryMode = 2;    //持久化消息
                    Context.SendChannel.BasicPublish(Config.ExchangeName, Config.RouteKey, properties, msg_byte);
                }
            }
        }

        public void OnListening(bool delay)
        {
            Task.Factory.StartNew(()=>ListenInit(delay));
        }

        private void ListenInit(bool delay)
        {
            Context.ReceiveConnection = RabbitMQClientFactory.CreateConnection(Config);
            Context.ReceiveConnection.ConnectionShutdown += (o, e) =>
            {
                return;
            };

            Context.ReceiveChannel = RabbitMQClientFactory.CreateChannel(Context.ReceiveConnection);
            if (delay)
            {
                RabbitMQClientFactory.ExchangeDeclare(Context.ReceiveChannel, "exchange-direct", "direct", false, false);
                RabbitMQClientFactory.QueueBind(Context.ReceiveChannel, Config.QueueName, "exchange-direct", "routing-delay");
                var consumer = new EventingBasicConsumer(Context.ReceiveChannel);
                consumer.Received += Consumer;

                Context.ReceiveChannel.BasicQos(0, 100, false);
                Context.ReceiveChannel.BasicConsume(Config.QueueName, false, consumer);
            }
            else
            {
                RabbitMQClientFactory.ExchangeDeclare(Context.ReceiveChannel, Config.ExchangeName, "direct", false, false);
                RabbitMQClientFactory.QueueBind(Context.ReceiveChannel, Config.QueueName, Config.ExchangeName, Config.RouteKey);
                var consumer = new EventingBasicConsumer(Context.ReceiveChannel);
                consumer.Received += Consumer;

                Context.ReceiveChannel.BasicQos(0, 100, false);
                Context.ReceiveChannel.BasicConsume(Config.QueueName, false, consumer);
            }

        }

        private void Consumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var result = MessageFactory.GetMessageResult(e.Body);
                _action_message?.Invoke(result);    //触发外部侦听事件

                if(Context.ReceiveChannel.IsClosed == false)
                {
                    Context.ReceiveChannel.BasicAck(e.DeliveryTag, false);
                }
                //if (result.Status == false)
                //{
                //    Context.ReceiveChannel.BasicReject(e.DeliveryTag, true);
                //}
                //else if (Context.ReceiveChannel.IsClosed == false)
                //{
                //    Context.ReceiveChannel.BasicAck(e.DeliveryTag, false);
                //}
            }
            catch (Exception ex)
            {
            }
        }

        public void Dispose()
        {
            if (Context.SendConnection == null)
                return;

            if (Context.SendConnection.IsOpen)
                Context.SendConnection.Close();

            Context.SendConnection.Dispose();
        }
    }
}