using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Messaging.Core.Interfaces;

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
				var i = 0;
				while (i < payloads.Count())
				{
					var properties = channel.CreateBasicProperties();
					var payload = payloads.ElementAt(i);
					properties.CorrelationId = payload.CorrelationId ?? "";
					properties.ReplyTo = payload.ReplyTo ?? "";
					var body = Encoding.UTF8.GetBytes(payload.Body);

					try
					{
						channel.BasicPublish(_rabbitConfiguration.Exchange, _rabbitConfiguration.RoutingKey, true, properties, body);
						i++;
					}
					catch (Exception ex)
					{
						//Log exception
					}
				}
			}
		}
	}
}
