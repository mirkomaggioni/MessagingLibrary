using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Messaging.Core.Interfaces;
using Messaging.Core.Models;
using RabbitMQ.Client;

namespace Messaging.Core.Services
{
	public class RabbitService<TPublisher, TConsumer> where TPublisher : class, IRabbitPublisherSetup, new() where TConsumer : class, IRabbitConsumerSetup, new()
	{
		private readonly ConnectionFactory _connectionFactory;
		private readonly ConcurrentDictionary<Guid, TConsumer> _consumers = new ConcurrentDictionary<Guid, TConsumer>();

		public RabbitService(ConnectionFactory connectionFactory)
		{
			_connectionFactory = connectionFactory;
		}

		public void Publish(IEnumerable<Payload> payloads, string exchange, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_connectionFactory, exchange, routingKey, type, durable);
			var messagePublisher = new TPublisher();
			messagePublisher.Setup(rabbitConfiguration).Publish(payloads);
		}

		public void Get(string exchange, string queue, IRabbitMessageHandler messageHandler, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_connectionFactory, exchange, routingKey, type, durable, queue);
			using (var messageConsumer = new TConsumer())
			{
				messageConsumer.Setup(rabbitConfiguration).Get(messageHandler);
			}
		}

		public Guid Subscribe(string exchange, string queue, IRabbitMessageHandler messageHandler, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var rabbitConfiguration = new RabbitConfiguration(_connectionFactory, exchange, routingKey, type, durable, queue);
			var consumer = new TConsumer();
			var consumerId = Guid.NewGuid();
			_consumers.TryAdd(consumerId, consumer);
			consumer.Setup(rabbitConfiguration).Consume(messageHandler);

			return consumerId;
		}

		public void Unsubscribe(Guid consumerId)
		{
			var consumer = _consumers[consumerId];
			consumer.Dispose();
			_consumers.TryRemove(consumerId, out TConsumer _);
		}
	}
}
