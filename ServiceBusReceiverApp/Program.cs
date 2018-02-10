using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using ServiceBus.Interfaces;
using ServiceBus.Services;

namespace ServiceBusReceiverApp
{
	class Program
	{
		private static readonly string _serviceBusConnectionString = ConfigurationManager.ConnectionStrings["AzureServiceBus"].ConnectionString;
		private static readonly string _serviceBusQueue = "queuemmtest1";
		private static AzureServiceBusService _azureServiceBusService = new AzureServiceBusService(_serviceBusConnectionString);
		static IQueueClient queueClient;

		static void Main(string[] args)
		{
			MainAsync().GetAwaiter().GetResult();
		}


		static async Task MainAsync()
		{
			queueClient = new QueueClient(_serviceBusConnectionString, _serviceBusQueue);

			Console.WriteLine("======================================================");
			Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
			Console.WriteLine("======================================================");

			RegisterOnMessageHandlerAndReceiveMessages();

			Console.ReadKey();

			await queueClient.CloseAsync();
		}

		static void RegisterOnMessageHandlerAndReceiveMessages()
		{
			var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
			{
				MaxConcurrentCalls = 1,
				AutoComplete = false
			};

			queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
		}

		static async Task ProcessMessagesAsync(Message message, CancellationToken token)
		{
			Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
			await queueClient.CompleteAsync(message.SystemProperties.LockToken);
		}

		static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
		{
			Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
			var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
			Console.WriteLine("Exception context for troubleshooting:");
			Console.WriteLine($"- Endpoint: {context.Endpoint}");
			Console.WriteLine($"- Entity Path: {context.EntityPath}");
			Console.WriteLine($"- Executing Action: {context.Action}");
			return Task.CompletedTask;
		}


		//static async Task MainAsync()
		//{
		//	var messages = await _azureServiceBusService.ReceiveMessagesAsync<GenericMessage>(_serviceBusQueue);

		//	for (var i = 0; i < 10; i++)
		//	{
		//		var message = $"Message {i}";
		//		Console.WriteLine($"receiving message: {message}");
		//	}
		//}
	}

	public class GenericMessage : IGenericMessage
	{
		public long SequenceNumber { get; set; }
		public string Body { get; set; }
	}
}
