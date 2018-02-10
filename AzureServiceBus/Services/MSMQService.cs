using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading.Tasks;
using Messaging.Models;

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

		public IEnumerable<GenericMessage> ReadAllMessageFromQueue(string queueName)
		{
			var messageQueue = new MessageQueue(queueName);
			messageQueue.Formatter = new XmlMessageFormatter(new Type[] {typeof(string)});
			var messages = new List<GenericMessage>();
			var queueMessages = messageQueue.GetAllMessages();

			foreach (var message in queueMessages)
			{
				var genericMessage = new GenericMessage()
				{
					MessageId = message.Id,
					Body = message.Body.ToString()
				};

				messages.Add(genericMessage);
			}

			messageQueue.Purge();
			messageQueue.Close();

			return messages;
		}

		public async Task<IEnumerable<GenericMessage>> ReadMessageFromQueueAsync(string queueName)
		{
			var messageNumber = 0;
			MessageQueue messageQueue = new MessageQueue(queueName);
			messageQueue.Refresh();
			messageQueue.Formatter = new XmlMessageFormatter(new Type[] {typeof(string)});
			var messages = new List<GenericMessage>();

			await Task.Run(async () =>
			{
				ReceiveMessages(messageNumber, messageQueue, messages);

				await Task.Delay(20000);
			});

			messageQueue.Purge();
			messageQueue.Close();

			return messages;
		}

		private static void ReceiveMessages(int messageNumber, MessageQueue messageQueue, List<GenericMessage> messages)
		{
			messageQueue.BeginReceive(TimeSpan.FromSeconds(2), messageNumber, (asyncResult) =>
			{
				try
				{
					var message = messageQueue.EndReceive(asyncResult);

					messages.Add(new GenericMessage()
					{
						MessageId = message.Id,
						Body = message.Body.ToString()
					});

					messageNumber++;

					ReceiveMessages(messageNumber, messageQueue, messages);
				}
				catch (Exception) { }
			});
		}
	}
}
