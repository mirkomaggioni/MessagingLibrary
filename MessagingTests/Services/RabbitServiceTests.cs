using System;
using System.Collections.Generic;
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
		public void MessagesAreSent()
		{
			PublishMessages(new Payload() { Body = "test message" }, sharedExchange);
			Assert.IsTrue(true);
		}

		[Test]
		public void AllMessagesInTheQueueAreReaded()
		{
			var messageHandler = new RabbitMessageHandler();
			PublishMessages(new Payload() { Body = "test message" }, sharedExchange);
			_sut.Get(sharedExchange, "message-shared-queue", messageHandler);
			Assert.IsTrue(messageHandler.Payloads.Count > 0);
			Assert.AreEqual(messageHandler.Payloads.ElementAt(0).Body, "test message");
		}

		[Test]
		public void AllMessagesWithARoutingKeyAreReaded()
		{
		var messageHandler = new RabbitMessageHandler();
			PublishMessages(new Payload() { Body = "hr message" }, directExchange, hrRoutingKey, "direct");
			_sut.Get(directExchange, "message-hr-queue", messageHandler, hrRoutingKey, "direct");
			Assert.IsTrue(messageHandler.Payloads.Count > 0);
			Assert.AreEqual(messageHandler.Payloads.ElementAt(0).Body, "hr message");
		}

		[Test]
		public void AllMessagesWithAnotherRoutingKeyAreNotReaded()
		{
			var messageHandler = new RabbitMessageHandler();
			PublishMessages(new Payload() { Body = "hr message" }, directExchange, hrRoutingKey, "direct");
			_sut.Get(directExchange, "message-marketing-queue", messageHandler, marketingRoutingKey, "direct");
			Assert.AreEqual(messageHandler.Payloads.Count, 0);
		}

		[Test]
		public void AllMessagesInTheQueueAreConsumed()
		{
			var messageHandler = new RabbitMessageHandler();
			var publisherTask = Task.Run(() =>
			{
				PublishMessages(new Payload() { Body = "test message" }, sharedExchange);
			});

			var subscriberTask = Task.Run(() => _sut.Subscribe(sharedExchange, "consumer-queue", messageHandler));
			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask());
			_sut.Unsubscribe(subscriberTask.Result);

			Assert.IsTrue(messageHandler.Payloads.Count > 0);
		}

		[Test]
		public void AllMessagesWithARoutingKeyAreConsumed()
		{
			var messageHandler = new RabbitMessageHandler();
			var publisherTask = Task.Run(() =>
			{
				PublishMessages(new Payload() { Body = "hr message" }, directExchange, hrRoutingKey, "direct");
			});

			var subscriberTask = Task.Run(() => _sut.Subscribe(directExchange, "consumer-hr-queue", messageHandler, hrRoutingKey, "direct"));
			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask());
			_sut.Unsubscribe(subscriberTask.Result);

			Assert.IsTrue(messageHandler.Payloads.Count > 0);
		}

		[Test]
		public void AllMessagesWithAnotherRoutingKeyAreNotConsumed()
		{
			var messageHandler = new RabbitMessageHandler();
			var publisherTask = Task.Run(() =>
			{
				PublishMessages(new Payload() { Body = "hr message" }, directExchange, hrRoutingKey, "direct");
			});

			var subscriberTask = Task.Run(() => _sut.Subscribe(directExchange, "consumer-marketing-queue", messageHandler, marketingRoutingKey, "direct"));
			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask());
			_sut.Unsubscribe(subscriberTask.Result);

			Assert.AreEqual(messageHandler.Payloads.Count, 0);
		}

		private void PublishMessages(Payload payload, string exchange, string routingKey = "", string type = "fanout")
		{
			var payloads = new List<Payload>();
			for (int i = 0; i < 10; i++)
			{
				payloads.Add(payload);
			}

			_sut.Publish(payloads, exchange, routingKey, type);
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
