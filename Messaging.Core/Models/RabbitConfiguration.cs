using Messaging.Core.Interfaces;
using RabbitMQ.Client;

namespace Messaging.Core.Models
{
	public class RabbitConfiguration : IRabbitConfiguration
	{
		public RabbitConfiguration(ConnectionFactory connectionFactory, string exchange, string queue, string routingKey, string type, bool durable)
		{
			ConnectionFactory = connectionFactory;
			Exchange = exchange;
			Queue = queue;
			RoutingKey = routingKey;
			Type = type;
			Durable = durable;
		}

		public ConnectionFactory ConnectionFactory { get; }
		public string Exchange { get; }
		public string Queue { get; }
		public string RoutingKey { get; }
		public string Type { get; }
		public bool Durable { get; }
	}
}
