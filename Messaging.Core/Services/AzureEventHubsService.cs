using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Messaging.Core.Services
{
	public class AzureEventHubsService
	{
		private readonly string _eventHubConnectionString = ConfigurationManager.ConnectionStrings["AzureEventHub"].ConnectionString;
		private readonly string _storageConnectionString = ConfigurationManager.ConnectionStrings["AzureStorage"].ConnectionString;
		private readonly string _containerName = "commtest1";
		private EventProcessorHost _eventProcessorHost;
		private readonly AzureEventProcessorFactory _azureEventProcessorFactory;

		public AzureEventHubsService(AzureEventProcessorFactory azureEventProcessorFactory)
		{
			_azureEventProcessorFactory = azureEventProcessorFactory;
		}

		public async Task SendEventToHubAsync(string eventHubPath, string message)
		{
			var connectionStringBuilder = new EventHubsConnectionStringBuilder(_eventHubConnectionString)
			{
				EntityPath = eventHubPath
			};

			var client = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
			var data = new EventData(Encoding.UTF8.GetBytes(message));
			await client.SendAsync(data);
			await client.CloseAsync();
		}

		public async Task ConsumeEventsFromHubAsync(string eventHubPath)
		{
			var connectionStringBuilder = new EventHubsConnectionStringBuilder(_eventHubConnectionString)
			{
				EntityPath = eventHubPath
			};

			var client = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
			_eventProcessorHost = new EventProcessorHost(eventHubPath, PartitionReceiver.DefaultConsumerGroupName, _eventHubConnectionString, _storageConnectionString, _containerName);
			await _eventProcessorHost.RegisterEventProcessorFactoryAsync(_azureEventProcessorFactory);
		}

		public async Task StopConsumeEventsFromHub()
		{
			await _eventProcessorHost.UnregisterEventProcessorAsync();
		}
	}
}
