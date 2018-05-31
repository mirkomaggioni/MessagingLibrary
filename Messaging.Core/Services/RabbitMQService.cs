using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Messaging.Core.Interfaces;
using Messaging.Core.Models;
using RabbitMQ.Client;

namespace Messaging.Core.Services
{
	public class RabbitMQService
	{
		private readonly string _hostName = ConfigurationManager.ConnectionStrings["RabbitMQHostname"].ConnectionString;
		private readonly ConnectionFactory _factory;

		public RabbitMQService()
		{
			_factory = new ConnectionFactory()
			{
				HostName = _hostName,
				RequestedHeartbeat = 30,
				AutomaticRecoveryEnabled = true,
				NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
			};
		}

		public void Publish(GenericMessage message, string exchange, string routingKey = "", string type = "fanout", bool durable = false)
		{
			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				var properties = channel.CreateBasicProperties();
				properties.CorrelationId = message.CorrelationId ?? "";
				properties.ReplyTo = message.ReplyTo ?? "";

				channel.ExchangeDeclare(exchange, type, durable, false, null);
				var body = Encoding.UTF8.GetBytes(message.Body);
				channel.BasicPublish(exchange, routingKey, true, properties, body);
			}
		}

		public void Get(string exchange, string queue, IRabbitMessageHandler messageHandler, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_factory, exchange, queue, routingKey, type, durable);
			var messageConsumer = new RabbitConsumer();
			messageConsumer.Setup(rabbitConfiguration);
			messageConsumer.Get(messageHandler);
		}

		public async Task SubscribeAsync(string exchange, string queue, IRabbitMessageHandler messageHandler, CancellationTokenSource cancellationTokenSource, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_factory, exchange, queue, routingKey, type, durable);
			var messageConsumer = new RabbitConsumer();
			messageConsumer.Setup(rabbitConfiguration);
			await messageConsumer.ConsumeAsync(messageHandler, cancellationTokenSource);
		}
	}
}
