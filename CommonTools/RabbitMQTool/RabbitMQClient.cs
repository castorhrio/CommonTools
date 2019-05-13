using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.RabbitMQTool
{
    public delegate void ActionEvent(EventMessageResult result);

    public class RabbitMQClient
    {
        private static readonly Lazy<RabbitMQClient> _instance = new Lazy<RabbitMQClient>(() => new RabbitMQClient());
        private RabbitMQClient()
        {

        }

        public static RabbitMQClient Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}
