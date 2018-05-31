using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading.Tasks;
using Messaging.Core.Models;

namespace Messaging.Services
{
	public class MSMQService
	{
		public bool SendMessageToQueue(string queueName, string message)
		{
			MessageQueue messageQueue = new MessageQueue(queueName);
			Message queueMessage = new Message()
			{
				TimeToBeReceived = TimeSpan.FromMinutes(30),
				Body = message
			};

			messageQueue.Send(queueMessage);
			messageQueue.Close();

			return true;
		}

		public IEnumerable<Payload> ReadAllMessageFromQueue(string queueName)
		{
			var messageQueue = new MessageQueue(queueName);
			messageQueue.Formatter = new XmlMessageFormatter(new Type[] {typeof(string)});
			var payloads = new List<Payload>();
			foreach (var message in messageQueue.GetAllMessages())
			{
				var payload = new Payload()
				{
					MessageId = message.Id,
					Body = message.Body.ToString()
				};

				payloads.Add(payload);
			}

			messageQueue.Purge();
			messageQueue.Close();

			return payloads;
		}

		public async Task<IEnumerable<Payload>> ReadMessageFromQueueAsync(string queueName)
		{
			var messageNumber = 0;
			MessageQueue messageQueue = new MessageQueue(queueName);
			messageQueue.Refresh();
			messageQueue.Formatter = new XmlMessageFormatter(new Type[] {typeof(string)});
			var payloads = new List<Payload>();

			await Task.Run(async () =>
			{
				ReceiveMessages(messageNumber, messageQueue, payloads);

				await Task.Delay(20000);
			});

			messageQueue.Purge();
			messageQueue.Close();

			return payloads;
		}

		private static void ReceiveMessages(int messageNumber, MessageQueue messageQueue, List<Payload> payloads)
		{
			messageQueue.BeginReceive(TimeSpan.FromSeconds(2), messageNumber, (asyncResult) =>
			{
				try
				{
					var message = messageQueue.EndReceive(asyncResult);

					payloads.Add(new Payload()
					{
						MessageId = message.Id,
						Body = message.Body.ToString()
					});

					messageNumber++;

					ReceiveMessages(messageNumber, messageQueue, payloads);
				}
				catch (Exception) { }
			});
		}
	}
}
