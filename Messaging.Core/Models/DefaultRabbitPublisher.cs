using Messaging.Core.Interfaces;
using System.Text;

namespace Messaging.Core.Models
{
	public class DefaultRabbitPublisher : IRabbitPublisher
	{
		private RabbitConfiguration _rabbitConfiguration;

		public void Setup(RabbitConfiguration rabbitConfiguration)
		{
			_rabbitConfiguration = rabbitConfiguration;
		}

		public void Publish(Payload payload)
		{
			using (var connection = _rabbitConfiguration.ConnectionFactory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				var properties = channel.CreateBasicProperties();
				properties.CorrelationId = payload.CorrelationId ?? "";
				properties.ReplyTo = payload.ReplyTo ?? "";

				channel.ExchangeDeclare(_rabbitConfiguration.Exchange, _rabbitConfiguration.Type, _rabbitConfiguration.Durable, false, null);
				var body = Encoding.UTF8.GetBytes(payload.Body);
				channel.BasicPublish(_rabbitConfiguration.Exchange, _rabbitConfiguration.RoutingKey, true, properties, body);
			}
		}
	}
}
