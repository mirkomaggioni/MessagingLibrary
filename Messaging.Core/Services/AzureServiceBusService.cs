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

		public async Task<IEnumerable<Payload>> ReadAllMessageFromQueueAsync(string queueName)
		{
			var subscriber = await _factory.CreateMessageReceiverAsync(queueName);
			var payloads = new List<Payload>();
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

						var payload = new Payload();
						payload.MessageId = message.MessageId;
						payload.Label = message.Label;
						payload.Body = body;
						payloads.Add(payload);
						await message.CompleteAsync();
					}
				}
				catch (Exception)
				{
					await message.DeadLetterAsync();
				}
			}

			await subscriber.CloseAsync();
			return payloads;
		}

		public async Task<IEnumerable<Payload>> ReadMessageFromQueueAsync(string queueName)
		{
			var payloads = new List<Payload>();
			var subscriber = _factory.CreateQueueClient(queueName);

			subscriber.OnMessageAsync(
				(Func<BrokeredMessage, Task>)(async message =>
				{
					using (var stream = message.GetBody<Stream>())
					using (var memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						var body = Encoding.UTF8.GetString(memoryStream.ToArray());

						var payload = new Payload();
						payload.MessageId = message.MessageId;
						payload.Label = message.Label;
						payload.Body = body;
						payloads.Add(payload);
						await message.CompleteAsync();
					};
				}));

			await Task.Delay(10000);

			await subscriber.CloseAsync();
			return payloads;
		}
	}
}
