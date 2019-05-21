using System;

namespace CommonTools.RabbitMQTool
{
    public interface IRabbitMQClient : IDisposable
    {
        /// <summary>
        /// MQ配置
        /// </summary>
        RabbitMQConfig Config { get; set; }

        /// <summary>
        /// MQ上下文
        /// <summary>
        RabbitMQClientContext Context { get; set; }

        /// <summary>
        /// 消息事件
        /// </summary>
        event ActionEvent ActionEvent;


        void Sender(MessageData message);

        void OnListening();

        void DelaySender(MessageData message, int delay_time);

        void DelayListening();
    }
}