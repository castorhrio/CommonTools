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

        /// <summary>
        /// 事件触发器
        /// </summary>
        /// <param name="message">消息实体</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        void EventTrigger(EventMessage message);

        /// <summary>
        /// 消息队列监听器
        /// </summary>
        void OnListening();
    }
}