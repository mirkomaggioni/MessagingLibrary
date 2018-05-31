using System;
using System.Configuration;
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

		public void Publish(Payload payload, string exchange, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_factory, exchange, routingKey, type, durable);
			var messagePublisher = new RabbitPublisher();
			messagePublisher.Setup(rabbitConfiguration);
			messagePublisher.Publish(payload);
		}

		public void Get(string exchange, string queue, IRabbitMessageHandler messageHandler, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_factory, exchange, routingKey, type, durable, queue);
			var messageConsumer = new RabbitConsumer();
			messageConsumer.Setup(rabbitConfiguration);
			messageConsumer.Get(messageHandler);
		}

		public async Task SubscribeAsync(string exchange, string queue, IRabbitMessageHandler messageHandler, CancellationTokenSource cancellationTokenSource, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_factory, exchange, routingKey, type, durable, queue);
			var messageConsumer = new RabbitConsumer();
			messageConsumer.Setup(rabbitConfiguration);
			await messageConsumer.ConsumeAsync(messageHandler, cancellationTokenSource);
		}
	}
}
