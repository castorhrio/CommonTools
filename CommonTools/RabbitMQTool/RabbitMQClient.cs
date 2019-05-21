using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.RabbitMQTool
{
    public enum ENUM_MQ_CLIENT_TYPE
    {
        Normal = 0,
        Delay = 1
    }

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

            set { _client = value; }
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

        #region 正常队列
        public void Sender(MessageData message)
        {
            Context.SendConnection = RabbitMQClientFactory.CreateConnection(Config);
            using (Context.SendConnection)
            {
                Context.SendChannel = RabbitMQClientFactory.CreateChannel(Context.SendConnection);
                using (Context.SendChannel)
                {
                    RabbitMQClientFactory.QueueDeclare(Context.SendChannel, Config.QueueName, true, false, false);
                    byte[] msg_byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    IBasicProperties properties = Context.SendChannel.CreateBasicProperties();
                    properties.DeliveryMode = 2;    //持久化消息
                    Context.SendChannel.BasicPublish(Config.ExchangeName, Config.RouteKey, properties, msg_byte);
                }
            }
        }

        public void OnListening()
        {
            Task.Factory.StartNew(Receiver);
        }

        private void Receiver()
        {
            Context.ReceiveConnection = RabbitMQClientFactory.CreateConnection(Config);
            Context.ReceiveConnection.ConnectionShutdown += (o, e) =>
            {
                return;
            };

            Context.ReceiveChannel = RabbitMQClientFactory.CreateChannel(Context.ReceiveConnection);
            RabbitMQClientFactory.ExchangeDeclare(Context.ReceiveChannel, Config.ExchangeName, "direct", false, false);
            RabbitMQClientFactory.QueueBind(Context.ReceiveChannel, Config.QueueName, Config.ExchangeName, Config.RouteKey);
            var consumer = new EventingBasicConsumer(Context.ReceiveChannel);
            consumer.Received += Consumer;

            Context.ReceiveChannel.BasicQos(0, 1, false);
            Context.ReceiveChannel.BasicConsume(Config.QueueName, false, consumer);
        }
        #endregion

        #region 延时队列
        public void DelaySender(MessageData message,int delay_time)
        {
            Context.SendConnection = RabbitMQClientFactory.CreateConnection(Config);
            using (Context.SendConnection)
            {
                Context.SendChannel = RabbitMQClientFactory.CreateChannel(Context.SendConnection);
                using (Context.SendChannel)
                {
                    string delay_exchange = Config.ExchangeName + "_Delay";
                    string delay_routekey = Config.RouteKey + "_Delay";
                    string delay_queue = Config.QueueName + "_Delay";
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    //队列过期时间
                    //dic.Add("x-expires", 30000);
                    //队列上消息过期时间，应小于队列过期时间  
                    dic.Add("x-message-ttl", delay_time);
                    //过期消息转向路由  
                    dic.Add("x-dead-letter-exchange", delay_exchange);
                    //过期消息转向路由相匹配routingkey  
                    dic.Add("x-dead-letter-routing-key", delay_routekey);

                    RabbitMQClientFactory.QueueDeclare(Context.SendChannel, delay_queue, true, false, false, dic);
                    byte[] msg_byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    IBasicProperties properties = Context.SendChannel.CreateBasicProperties();
                    properties.DeliveryMode = 2;    //持久化消息
                    Context.SendChannel.BasicPublish("", delay_queue, properties, msg_byte);
                }
            }
        }

        public void DelayListening()
        {
            Task.Factory.StartNew(DelayReceiver);
        }

        private void DelayReceiver()
        {
            Context.ReceiveConnection = RabbitMQClientFactory.CreateConnection(Config);
            Context.ReceiveConnection.ConnectionShutdown += (o, e) =>
            {
                return;
            };

            Context.ReceiveChannel = RabbitMQClientFactory.CreateChannel(Context.ReceiveConnection);
            string delay_exchange = Config.ExchangeName + "_Delay";
            string delay_routekey = Config.RouteKey + "_Delay";
            RabbitMQClientFactory.ExchangeDeclare(Context.ReceiveChannel, delay_exchange, "direct", false, false);
            string delay_queue = Context.ReceiveChannel.QueueDeclare().QueueName;
            RabbitMQClientFactory.QueueBind(Context.ReceiveChannel, delay_queue, delay_exchange, delay_routekey);
            var consumer = new EventingBasicConsumer(Context.ReceiveChannel);
            consumer.Received += Consumer;

            Context.ReceiveChannel.BasicQos(0, 1, false);
            Context.ReceiveChannel.BasicConsume(delay_queue, false, consumer);
        }
        #endregion

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