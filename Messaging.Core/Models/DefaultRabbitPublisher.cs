using Messaging.Core.Interfaces;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Core.Models
{
	public class DefaultRabbitPublisher : IRabbitPublisher
	{
		private RabbitConfiguration _rabbitConfiguration;

		public IRabbitPublisher Setup(RabbitConfiguration rabbitConfiguration)
		{
			_rabbitConfiguration = rabbitConfiguration;
			return this;
		}

		public void Publish(IEnumerable<Payload> payloads)
		{
			using (var connection = _rabbitConfiguration.ConnectionFactory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_rabbitConfiguration.Exchange, _rabbitConfiguration.Type, _rabbitConfiguration.Durable, false, null);

				foreach (var payload in payloads)
				{
					var properties = channel.CreateBasicProperties();
					properties.CorrelationId = payload.CorrelationId ?? "";
					properties.ReplyTo = payload.ReplyTo ?? "";

					var body = Encoding.UTF8.GetBytes(payload.Body);
					channel.BasicPublish(_rabbitConfiguration.Exchange, _rabbitConfiguration.RoutingKey, true, properties, body);
				}
			}
		}
	}
}
