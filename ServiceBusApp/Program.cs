using System;
using System.Configuration;
using System.Threading.Tasks;
using ServiceBus.Services;

namespace ServiceBusSenderApp
{
	class Program
	{
		private static readonly string _serviceBusConnectionString = ConfigurationManager.ConnectionStrings["AzureServiceBus"].ConnectionString;
		private static readonly string _serviceBusQueue = "queuemmtest1";
		private static AzureServiceBusService _azureServiceBusService = new AzureServiceBusService(_serviceBusConnectionString);

		static void Main(string[] args)
		{
			MainAsync().GetAwaiter().GetResult();
		}

		static async Task MainAsync()
		{
			for (var i = 0; i < 10; i++)
			{
				var message = $"Message {i}";
				Console.WriteLine($"Sending message: {message}");
				await _azureServiceBusService.SendMessageAsync(_serviceBusQueue, message);
			}
		}
	}
}
