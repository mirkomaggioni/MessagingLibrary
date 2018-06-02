using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Messaging.Core.Interfaces;
using Messaging.Core.Models;
using RabbitMQ.Client;

namespace Messaging.Core.Services
{
	public class RabbitService<TPublisher, TConsumer> where TPublisher : class, IRabbitPublisher, new() where TConsumer : class, IRabbitConsumer, new()
	{
		private readonly ConnectionFactory _connectionFactory;

		public RabbitService(ConnectionFactory connectionFactory)
		{
			_connectionFactory = connectionFactory;
		}

		public void Publish(Payload payload, string exchange, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_connectionFactory, exchange, routingKey, type, durable);
			var messagePublisher = new TPublisher();
			messagePublisher.Setup(rabbitConfiguration);
			messagePublisher.Publish(payload);
		}

		public void Get(string exchange, string queue, IRabbitMessageHandler messageHandler, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_connectionFactory, exchange, routingKey, type, durable, queue);
			var messageConsumer = new TConsumer();
			messageConsumer.Setup(rabbitConfiguration);
			messageConsumer.Get(messageHandler);
		}

		public async Task SubscribeAsync(string exchange, string queue, IRabbitMessageHandler messageHandler, CancellationTokenSource cancellationTokenSource, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_connectionFactory, exchange, routingKey, type, durable, queue);
			var messageConsumer = new TConsumer();
			messageConsumer.Setup(rabbitConfiguration);
			await messageConsumer.ConsumeAsync(messageHandler, cancellationTokenSource);
		}
	}
}
