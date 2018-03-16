using Microsoft.Azure.EventHubs.Processor;

namespace Messaging.Core.Services
{
	public class AzureEventProcessorFactory : IEventProcessorFactory
	{
		private readonly GenericEventsService _genericEventsService;

		public AzureEventProcessorFactory(GenericEventsService genericEventsService)
		{
			_genericEventsService = genericEventsService;
		}

		IEventProcessor IEventProcessorFactory.CreateEventProcessor(PartitionContext context)
		{
			return new EventProcessor(context, _genericEventsService);
		}
	}
}
