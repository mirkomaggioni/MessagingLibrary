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
		private readonly string exchange = "messages";
		private readonly string queue = "defaultqueue";

		[SetUp]
		public void Setup()
		{
			_sut = new RabbitMQService();
		}

		[Test]
		public void MessagesAreSent()
		{
			for (int i = 0; i < 10; i++)
			{
				_sut.Publish("message", exchange: exchange);
			}

			Assert.IsTrue(true);
		}

		[Test]
		public void AllMessagesInTheQueueAreReaded()
		{
			var messages = _sut.Get(exchange, queue);
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.ElementAt(0).Body, "message");
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
				for (int i = 0; i < 10; i++)
				{
					_sut.Publish("test message", exchange);
				}
			});

			var cancellationTokenSource = new CancellationTokenSource();
			var subscriberTask = Task.Run(() =>
			{
				_sut.SubscribeAsync(exchange, queue, callback, cancellationTokenSource).ConfigureAwait(false);
			});

			var cancelSubscriberTask = Task.Run(async () =>
			{
				await Task.Delay(10000).ConfigureAwait(false);
				cancellationTokenSource.Cancel();
			});

			Task.WaitAll(publisherTask, subscriberTask, cancelSubscriberTask);

			Assert.That(messages.Count == 10);
		}
	}
}
