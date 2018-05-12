using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;
using Messaging.Core.Models;

namespace Messaging.Services
{
	public class AzureServiceBusService
	{
		private readonly string _serviceBusConnectionString = ConfigurationManager.ConnectionStrings["AzureServiceBus"].ConnectionString;
		private readonly MessagingFactory _factory;
		public AzureServiceBusService()
		{
			_factory = MessagingFactory.CreateFromConnectionString(_serviceBusConnectionString);
		}

		public async Task<bool> SendMessageToQueueAsync(string queueName, string message)
		{
			var sender = await _factory.CreateMessageSenderAsync(queueName);

			var brokeredMessage = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(message)))
			{
				ContentType = "application/json",
				Label = "",
				MessageId = Guid.NewGuid().ToString(),
				TimeToLive = TimeSpan.FromMinutes(30)
			};

			await sender.SendAsync(brokeredMessage);

			await sender.CloseAsync();
			return true;
		}

		public async Task<IEnumerable<GenericMessage>> ReadAllMessageFromQueueAsync(string queueName)
		{
			var subscriber = await _factory.CreateMessageReceiverAsync(queueName);
			var messages = new List<GenericMessage>();
			BrokeredMessage message = null;

			while ((message = await subscriber.ReceiveAsync(new TimeSpan(hours: 0, minutes: 0, seconds: 5))) != null)
			{
				try
				{
					using (var stream = message.GetBody<Stream>())
					using (var memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						var body = Encoding.UTF8.GetString(memoryStream.ToArray());

						var receivedMessage = new GenericMessage();
						receivedMessage.MessageId = message.MessageId;
						receivedMessage.Label = message.Label;
						receivedMessage.Body = body;
						messages.Add(receivedMessage);
						await message.CompleteAsync();
					}
				}
				catch (Exception ex)
				{
					//Log
					await message.DeadLetterAsync();
				}
			}

			await subscriber.CloseAsync();
			return messages;
		}

		public async Task<IEnumerable<GenericMessage>> ReadMessageFromQueueAsync(string queueName)
		{
			var messages = new List<GenericMessage>();
			var subscriber = _factory.CreateQueueClient(queueName);

			subscriber.OnMessageAsync(
				async message =>
				{
					using (var stream = message.GetBody<Stream>())
					using (var memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						var body = Encoding.UTF8.GetString(memoryStream.ToArray());

						var receivedMessage = new GenericMessage();
						receivedMessage.MessageId = message.MessageId;
						receivedMessage.Label = message.Label;
						receivedMessage.Body = body;
						messages.Add(receivedMessage);
						await message.CompleteAsync();
					};
				});

			await Task.Delay(10000);

			await subscriber.CloseAsync();
			return messages;
		}
	}
}
