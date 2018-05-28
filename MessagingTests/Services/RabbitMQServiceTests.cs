using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Messaging.Core.Interfaces;
using Messaging.Core.Models;
using Messaging.Core.Models.Rabbit;
using Messaging.Core.Services;
using NUnit.Framework;

namespace ServiceBusTests.Services
{
	public class RabbitMQServiceTests
	{
		private RabbitMQService _sut;
		private IMessageHandler _messageHandler;
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
			_messageHandler = new MessageHandler();
		}

		[Test]
		public async Task MessagesAreSentAsync()
		{
			await PublishMessagesAsync(new GenericMessage() { Body = "test message" }, sharedExchange).ConfigureAwait(false);
			Assert.IsTrue(true);
		}

		[Test]
		public async Task AllMessagesInTheQueueAreReadedAsync()
		{
			await PublishMessagesAsync(new GenericMessage() { Body = "test message" }, sharedExchange).ConfigureAwait(false);
			var messages = _sut.Get(sharedExchange, sharedQueue);
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.ElementAt(0).Body, "test message");
		}

		[Test]
		public async Task AllMessagesWithARoutingKeyAreReadedAsync()
		{
			await PublishMessagesAsync(new GenericMessage() { Body = "hr message" }, directExchange, hrRoutingKey, "direct").ConfigureAwait(false);
			var messages = _sut.Get(directExchange, hrQueue, hrRoutingKey, "direct");
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.ElementAt(0).Body, "hr message");
		}

		[Test]
		public async Task AllMessagesWithAnotherRoutingKeyAreNotReadedAsync()
		{
			await PublishMessagesAsync(new GenericMessage() { Body = "marketing message" }, directExchange, marketingRoutingKey, "direct").ConfigureAwait(false);
			var messages = _sut.Get(directExchange, hrQueue, hrRoutingKey, "direct");
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.Count(), 0);
		}

		[Test]
		public void AllMessagesInTheQueueAreConsumed()
		{
			var publisherTask = Task.Run(async () =>
			{
				await PublishMessagesAsync(new GenericMessage() { Body = "test message" }, sharedExchange).ConfigureAwait(false);
			});

			var cancellationTokenSource = new CancellationTokenSource();
			var subscriberTask = Task.Run(() =>
			{
				_sut.SubscribeAsync(sharedExchange, consumerQueue, _messageHandler, cancellationTokenSource).ConfigureAwait(false);
			});

			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask(cancellationTokenSource));

			Assert.AreEqual(_messageHandler.Messages.Count, 10);
		}

		[Test]
		public void AllMessagesWithARoutingKeyAreConsumed()
		{
			var publisherTask = Task.Run(async () =>
			{
				await PublishMessagesAsync(new GenericMessage() { Body = "hr message" }, directExchange, hrRoutingKey, "direct").ConfigureAwait(false);
			});

			var cancellationTokenSource = new CancellationTokenSource();
			var subscriberTask = Task.Run(() =>
			{
				_sut.SubscribeAsync(directExchange, hrConsumerQueue, _messageHandler, cancellationTokenSource, hrRoutingKey, "direct").ConfigureAwait(false);
			});

			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask(cancellationTokenSource));

			Assert.AreEqual(_messageHandler.Messages.Count, 10);
		}

		[Test]
		public void AllMessagesWithAnotherRoutingKeyAreNotConsumed()
		{
			var publisherTask = Task.Run(async () =>
			{
				await PublishMessagesAsync(new GenericMessage() { Body = "marketing message" }, directExchange, marketingRoutingKey, "direct").ConfigureAwait(false);
			});

			var cancellationTokenSource = new CancellationTokenSource();
			var subscriberTask = Task.Run(() =>
			{
				_sut.SubscribeAsync(sharedExchange, hrConsumerQueue, _messageHandler, cancellationTokenSource, hrRoutingKey, "direct").ConfigureAwait(false);
			});

			Task.WaitAll(publisherTask, subscriberTask, CancelSubscriberTask(cancellationTokenSource));

			Assert.AreEqual(_messageHandler.Messages.Count, 0);
		}

		private async Task PublishMessagesAsync(GenericMessage message, string exchange, string routingKey = "", string type = "fanout")
		{
			var i = 0;
			var published = false;
			while (i < 10)
			{
				try
				{
					_sut.Publish(message, exchange, routingKey, type);
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
