using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.RabbitMQTool
{
    public class EventMessage
    {
        public string EventMessageMarkCode { get; set; }

        public byte[] EventMessageBytes { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class EventMessageResult
    {
        public EventMessage EventMessage { get; set; }

        public byte[] Messages { get; set; }

        public bool Status { get; set; }

        internal static EventMessageResult GetEventMessageResult(byte[] bytes)
        {
            EventMessageResult result = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    EventMessage message = JsonConvert.DeserializeObject<EventMessage>(ms.ToString());
                    result.EventMessage = message;

                    if (message != null)
                    {
                        result.Messages = message.EventMessageBytes;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }

    public class EventMessageFactory
    {
        public static EventMessage CreateEventMessageInstance<T>(T obj,string code) where T:class,new()
        {
            try
            {
                EventMessage event_msg = new EventMessage
                {
                    CreateTime = DateTime.Now,
                    EventMessageMarkCode = code,
                    EventMessageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj))
                };

                return event_msg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
