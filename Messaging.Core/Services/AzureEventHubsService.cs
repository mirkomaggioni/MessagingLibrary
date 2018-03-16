using System;
using System.Collections.Generic;
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
			await _eventProcessorHost.RegisterEventProcessorAsync<EventProcessor>();
		}

		public async Task StopConsumeEventsFromHub()
		{
			await _eventProcessorHost.UnregisterEventProcessorAsync();
		}
	}

	public class EventProcessor : IEventProcessor
	{
		public Task CloseAsync(PartitionContext context, CloseReason reason)
		{
			return Task.CompletedTask;
		}

		public Task OpenAsync(PartitionContext context)
		{
			return Task.CompletedTask;
		}

		public Task ProcessErrorAsync(PartitionContext context, Exception error)
		{
			return Task.CompletedTask;
		}

		public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
		{
			foreach (var message in messages)
			{
				var data = Encoding.UTF8.GetString(message.Body.Array, message.Body.Offset, message.Body.Count);
				Console.WriteLine(data);
			}

			return context.CheckpointAsync();
		}
	}
}
