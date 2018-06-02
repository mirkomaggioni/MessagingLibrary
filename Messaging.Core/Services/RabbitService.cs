using System;
using System.Collections.Concurrent;
using Messaging.Core.Interfaces;
using Messaging.Core.Models;
using RabbitMQ.Client;

namespace Messaging.Core.Services
{
	public class RabbitService<TPublisher, TConsumer> where TPublisher : class, IRabbitPublisher, new() where TConsumer : class, IRabbitConsumer, new()
	{
		private readonly ConnectionFactory _connectionFactory;
		private readonly ConcurrentDictionary<Guid, TConsumer> _consumers = new ConcurrentDictionary<Guid, TConsumer>();

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

		public Guid Subscribe(string exchange, string queue, IRabbitMessageHandler messageHandler, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_connectionFactory, exchange, routingKey, type, durable, queue);
			var consumer = new TConsumer();
			consumer.Setup(rabbitConfiguration);
			var consumerId = Guid.NewGuid();
			_consumers.TryAdd(consumerId, consumer);
			consumer.Consume(messageHandler);

			return consumerId;
		}

		public void Dispose(Guid consumerId)
		{
			var consumer = _consumers[consumerId];
			consumer.Dispose();
			_consumers.TryRemove(consumerId, out TConsumer _);
		}
	}
}
