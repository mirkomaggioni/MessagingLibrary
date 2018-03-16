using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Messaging.Core.Services
{
	public class EventProcessor : IEventProcessor
	{
		private readonly GenericEventsService _genericEventsService;
		private readonly PartitionContext _context;

		public EventProcessor(PartitionContext context, GenericEventsService genericEventsService)
		{
			_genericEventsService = genericEventsService;
			_context = context;
		}

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
				_genericEventsService.AddEvent(data);
			}

			return context.CheckpointAsync();
		}
	}
}
