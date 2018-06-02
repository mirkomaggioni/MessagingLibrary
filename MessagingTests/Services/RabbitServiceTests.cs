using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Messaging.Core;
using Messaging.Core.Models;
using Messaging.Core.Models.Rabbit;
using Messaging.Core.Services;
using NUnit.Framework;

namespace ServiceBusTests.Services
{
	public class RabbitServiceTests
	{
		private RabbitService<DefaultRabbitPublisher, DefaultRabbitConsumer> _sut;
		private readonly string sharedExchange = "message.shared";
		private readonly string directExchange = "message.direct";
		private readonly string sharedQueue = "message-shared-queue";
		private readonly string consumerQueue = "message-consumer-queue";
		private readonly string hrQueue = "message-hr-queue";
		private readonly string hrConsumerQueue = "message-hr-consumer-queue";
		private readonly string hrRoutingKey = "message.hr";
		private readonly string marketingRoutingKey = "message.marketing";

		[SetUp]
		public void Setup()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new MessagingCoreModule());

			using (var container = containerBuilder.Build())
			{
				_sut = container.Resolve<RabbitService<DefaultRabbitPublisher, DefaultRabbitConsumer>>();
			}
		}

		[Test]
		public async Task MessagesAreSentAsync()
		{
			await PublishMessagesAsync(new Payload() { Body = "test message" }, sharedExchange).ConfigureAwait(false);
			Assert.IsTrue(true);
		}

		[Test]
		public async Task AllMessagesInTheQueueAreReadedAsync()
		{
			var messageHandler = new RabbitMessageHandler();
			await PublishMessagesAsync(new Payload() { Body = "test message" }, sharedExchange).ConfigureAwait(false);
			_sut.Get(sharedExchange, sharedQueue, messageHandler);
			Assert.IsTrue(messageHandler.Payloads.Count > 0);
			Assert.AreEqual(messageHandler.Payloads.ElementAt(0).Body, "test message");
		}

		[Test]
		public async Task AllMessagesWithARoutingKeyAreReadedAsync()
		{
			var messageHandler = new RabbitMessageHandler();
			await PublishMessagesAsync(new Payload() { Body = "hr message" }, directExchange, hrRoutingKey, "direct").ConfigureAwait(false);
			_sut.Get(directExchange, hrQueue, messageHandler, hrRoutingKey, "direct");
			Assert.IsTrue(messageHandler.Payloads.Count > 0);
			Assert.AreEqual(messageHandler.Payloads.ElementAt(0).Body, "hr message");
		}

		[Test]
		public async Task AllMessagesWithAnotherRoutingKeyAreNotReadedAsync()
		{
			var messageHandler = new RabbitMessageHandler();
			await PublishMessagesAsync(new Payload() { Body = "marketing message" }, directExchange, marketingRoutingKey, "direct").ConfigureAwait(false);
			_sut.Get(directExchange, hrQueue, messageHandler, hrRoutingKey, "direct");
			Assert.AreEqual(messageHandler.Payloads.Count, 0);
		}

		[Test]
		public void AllMessagesInTheQueueAreConsumed()
		{
			var messageHandler = new RabbitMessageHandler();
			var publisherTask = Task.Run(async () =>
			{
				await PublishMessagesAsync(new Payload() { Body = "test message" }, sharedExchange).ConfigureAwait(false);
			});

			var subscriberTask = Task.Run(() => _sut.Subscribe(sharedExchange, consumerQueue, messageHandler));
			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask());
			_sut.Dispose(subscriberTask.Result);

			Assert.AreEqual(messageHandler.Payloads.Count, 10);
		}

		[Test]
		public void AllMessagesWithARoutingKeyAreConsumed()
		{
			var messageHandler = new RabbitMessageHandler();
			var publisherTask = Task.Run(async () =>
			{
				await PublishMessagesAsync(new Payload() { Body = "hr message" }, directExchange, hrRoutingKey, "direct").ConfigureAwait(false);
			});

			var subscriberTask = Task.Run(() => _sut.Subscribe(directExchange, hrConsumerQueue, messageHandler, hrRoutingKey, "direct"));
			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask());
			_sut.Dispose(subscriberTask.Result);

			Assert.AreEqual(messageHandler.Payloads.Count, 10);
		}

		[Test]
		public void AllMessagesWithAnotherRoutingKeyAreNotConsumed()
		{
			var messageHandler = new RabbitMessageHandler();
			var publisherTask = Task.Run(async () =>
			{
				await PublishMessagesAsync(new Payload() { Body = "marketing message" }, directExchange, marketingRoutingKey, "direct").ConfigureAwait(false);
			});

			var subscriberTask = Task.Run(() => _sut.Subscribe(sharedExchange, hrConsumerQueue, messageHandler, hrRoutingKey, "direct"));
			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask());
			_sut.Dispose(subscriberTask.Result);

			Assert.AreEqual(messageHandler.Payloads.Count, 0);
		}

		private async Task PublishMessagesAsync(Payload payload, string exchange, string routingKey = "", string type = "fanout")
		{
			var i = 0;
			var published = false;
			while (i < 10)
			{
				try
				{
					_sut.Publish(payload, exchange, routingKey, type);
					published = true;
				}
				catch (Exception)
				{
					await Task.Delay(3000).ConfigureAwait(false);
					published = false;
				}
				finally
				{
					if (published)
						i++;
				}
			}
		}

		private Task CancelSubscriberTask()
		{
			return Task.Run(async () =>
			{
				await Task.Delay(5000).ConfigureAwait(false);
			});
		}
	}
}
