using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommonTools.RabbitMQTool
{
    public class MessageFactory
    {
        public static byte[] SerializerMessageToBytes<T>(T message) where T : class, new()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                using(MemoryStream ms = new MemoryStream())
                {
                    xml.Serialize(ms, message);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 序列化消息为xml字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string SerializerMessageToString<T>(T message) where T : class, new()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream())
                {
                    xml.Serialize(ms, message);
                    return ms.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 反序列化消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T DeserlializeMessage<T>(byte[] bytes) where T : class, new()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return JsonConvert.DeserializeObject<T>(ms.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
