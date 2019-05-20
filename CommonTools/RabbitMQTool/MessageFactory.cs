using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace CommonTools.RabbitMQTool
{
    public class MessageData
    {
        public string Id { get; set; }

        public DateTime CreateTime { get; set; }

        public byte[] DataBytes { get; set; }
    }

    public class MessageResult
    {
        public MessageData MessageData { get; set; }

        public string Messages { get; set; }
    }

    public class MessageFactory
    {
        public static MessageData CreateMessageInstance<T>(T obj) where T : class, new()
        {
            try
            {
                MessageData event_msg = new MessageData
                {
                    Id = Guid.NewGuid().ToString("N"),
                    CreateTime = DateTime.Now,
                    DataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj))
                };

                return event_msg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static MessageResult GetMessageResult(byte[] bytes)
        {
            MessageResult result = new MessageResult();
            try
            {
                MessageData message = JsonConvert.DeserializeObject<MessageData>(Encoding.UTF8.GetString(bytes));
                result.MessageData = message;

                if (message != null)
                {
                    result.Messages = Encoding.UTF8.GetString(message.DataBytes);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}