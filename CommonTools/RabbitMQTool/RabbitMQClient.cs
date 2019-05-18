using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.RabbitMQTool
{
    public delegate void ActionEvent(EventMessageResult result);

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

        public void EventTrigger(EventMessage message)
        {
            Context.SendConnection = RabbitMQClientFactory.CreateConnection(Config);
            using (Context.SendConnection)
            {
                Context.SendChannel = RabbitMQClientFactory.CreateChannel(Context.SendConnection);
                using (Context.SendChannel)
                {
                    byte[] msg_byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    IBasicProperties properties = Context.SendChannel.CreateBasicProperties();
                    properties.DeliveryMode = 2;    //持久化消息
                    Context.SendChannel.BasicPublish(Config.ExchangeName, Config.RouteKey, properties, msg_byte);
                }
            }
        }

        public void OnListening()
        {
            Task.Factory.StartNew(ListenInit);
        }

        private void ListenInit()
        {
            Context.ReceiveConnection = RabbitMQClientFactory.CreateConnection(Config);
            Context.ReceiveConnection.ConnectionShutdown += (o, e) =>
            {
            };

            Context.ReceiveChannel = RabbitMQClientFactory.CreateChannel(Context.ReceiveConnection);
            var consumer = new EventingBasicConsumer(Context.ReceiveChannel);
            consumer.Received += Consumer;

            Context.ReceiveChannel.BasicQos(0, 1000, false);
            Context.ReceiveChannel.BasicConsume(Context.QueueName, false, consumer);
        }

        private void Consumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var result = EventMessageResult.GetEventMessageResult(e.Body);
                _action_message?.Invoke(result);    //触发外部侦听事件

                if (result.Status == false)
                {
                    Context.ReceiveChannel.BasicReject(e.DeliveryTag, true);
                }
                else if (Context.ReceiveChannel.IsClosed == false)
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