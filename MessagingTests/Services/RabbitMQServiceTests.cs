using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Messaging.Core.Models;
using Messaging.Core.Services;
using NUnit.Framework;

namespace ServiceBusTests.Services
{
	public class RabbitMQServiceTests
	{
		private RabbitMQService _sut;
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
			_sut = new RabbitMQService();
		}

		[Test]
		public void MessagesAreSent()
		{
			PublishMessages("test message", sharedExchange);
			Assert.IsTrue(true);
		}

		[Test]
		public void AllMessagesInTheQueueAreReaded()
		{
			PublishMessages("test message", sharedExchange);
			var messages = _sut.Get(sharedExchange, sharedQueue);
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.ElementAt(0).Body, "test message");
		}

		[Test]
		public void AllMessagesWithARoutingKeyAreReaded()
		{
			PublishMessages("hr message", directExchange, hrRoutingKey, "direct");
			var messages = _sut.Get(directExchange, hrQueue, hrRoutingKey, "direct");
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.ElementAt(0).Body, "hr message");
		}

		[Test]
		public void AllMessagesWithAnotherRoutingKeyAreNotReaded()
		{
			PublishMessages("marketing message", directExchange, marketingRoutingKey, "direct");
			var messages = _sut.Get(directExchange, hrQueue, hrRoutingKey, "direct");
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.Count(), 0);
		}

		[Test]
		public void AllMessagesInTheQueueAreConsumed()
		{
			var messages = new List<GenericMessage>();
			var callback = new Action<GenericMessage>((message) =>
			{
				messages.Add(message);
			});

			var publisherTask = Task.Run(() =>
			{
				PublishMessages("test message", sharedExchange);
			});

			var cancellationTokenSource = new CancellationTokenSource();
			var subscriberTask = Task.Run(() =>
			{
				_sut.SubscribeAsync(sharedExchange, consumerQueue, callback, cancellationTokenSource).ConfigureAwait(false);
			});

			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask(cancellationTokenSource));

			Assert.AreEqual(messages.Count, 10);
		}

		[Test]
		public void AllMessagesWithARoutingKeyAreConsumed()
		{
			var messages = new List<GenericMessage>();
			var callback = new Action<GenericMessage>((message) =>
			{
				messages.Add(message);
			});

			var publisherTask = Task.Run(() =>
			{
				PublishMessages("hr message", directExchange, hrRoutingKey, "direct");
			});

			var cancellationTokenSource = new CancellationTokenSource();
			var subscriberTask = Task.Run(() =>
			{
				_sut.SubscribeAsync(directExchange, hrConsumerQueue, callback, cancellationTokenSource, hrRoutingKey, "direct").ConfigureAwait(false);
			});

			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask(cancellationTokenSource));

			Assert.AreEqual(messages.Count, 10);
		}

		[Test]
		public void AllMessagesWithAnotherRoutingKeyAreNotConsumed()
		{
			var messages = new List<GenericMessage>();
			var callback = new Action<GenericMessage>((message) =>
			{
				messages.Add(message);
			});

			var publisherTask = Task.Run(() =>
			{
				PublishMessages("marketing message", directExchange, marketingRoutingKey, "direct");
			});

			var cancellationTokenSource = new CancellationTokenSource();
			var subscriberTask = Task.Run(() =>
			{
				_sut.SubscribeAsync(sharedExchange, hrConsumerQueue, callback, cancellationTokenSource, hrRoutingKey, "direct").ConfigureAwait(false);
			});

			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask(cancellationTokenSource));

			Assert.AreEqual(messages.Count, 0);
		}

		private void PublishMessages(string message, string exchange, string routingKey = "", string type = "fanout")
		{
			for (int i = 0; i < 10; i++)
			{
				_sut.Publish(message, exchange, routingKey, type);
			}
		}

		private Task CancelSubscriberTask(CancellationTokenSource cancellationTokenSource)
		{
			return Task.Run(async () =>
			{
				await Task.Delay(5000).ConfigureAwait(false);
				cancellationTokenSource.Cancel();
			});
		}
	}
}
