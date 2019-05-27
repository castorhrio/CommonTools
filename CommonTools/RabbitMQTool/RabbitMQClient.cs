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
                    RabbitMQClientFactory.ExchangeDeclare(Context.SendChannel, Config.ExchangeName, "direct", false, false);
                    RabbitMQClientFactory.QueueDeclare(Context.SendChannel, Config.QueueName, true, false, false);
                    RabbitMQClientFactory.QueueBind(Context.SendChannel, Config.QueueName, Config.ExchangeName, Config.RouteKey);

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
                    string queue = "test_ttl_queue";
                    string exchange = "test_ttl_exchange";
                    string route_key = "test_ttl_routekey";

                    RabbitMQClientFactory.ExchangeDeclare(Context.SendChannel, exchange, "direct", false, false);
                    Dictionary<string, object> arg = new Dictionary<string, object>();
                    arg.Add("x-dead-letter-exchange", "test_ttl_exchange.delay");
                    arg.Add("x-dead-letter-routing-key", "test_ttl_route.delay");
                    RabbitMQClientFactory.QueueDeclare(Context.SendChannel, queue, true, false, false, arg);
                    RabbitMQClientFactory.QueueBind(Context.SendChannel, queue, exchange, route_key);

                    byte[] msg_byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    IBasicProperties properties = Context.SendChannel.CreateBasicProperties();
                    properties.DeliveryMode = 2;    //持久化消息
                    properties.Expiration = delay_time.ToString(); //消息过期时间
                    Context.SendChannel.BasicPublish(exchange, route_key, properties, msg_byte);
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

            string delay_exchange = "test_ttl_exchange.delay";
            string delay_routekey = "test_ttl_route.delay";
            string delay_queue = "test_ttl_queue.delay";
            RabbitMQClientFactory.ExchangeDeclare(Context.ReceiveChannel, delay_exchange, "direct", false, false);
            RabbitMQClientFactory.QueueDeclare(Context.ReceiveChannel, delay_queue, true, false, false, null);
            RabbitMQClientFactory.QueueBind(Context.ReceiveChannel, delay_queue, delay_exchange, delay_routekey);

            var consumer = new EventingBasicConsumer(Context.ReceiveChannel);
            consumer.Received += Consumer;
            Context.ReceiveChannel.BasicQos(0, 10, false);
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