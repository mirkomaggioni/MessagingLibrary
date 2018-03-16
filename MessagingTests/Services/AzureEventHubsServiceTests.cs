using System.Threading.Tasks;
using Messaging.Core.Services;
using NUnit.Framework;

namespace ServiceBusTests.Services
{
	[TestFixture]
	public class AzureEventHubsServiceTests
	{
		private readonly string _eventHubPath = "ehmmtest1";
		private AzureEventHubsService _sut;
		private GenericEventsService _genericEventsService;

		[SetUp]
		public void Setup()
		{
			_genericEventsService = new GenericEventsService();
			var azureEventProcessorFactory = new AzureEventProcessorFactory(_genericEventsService);
			_sut = new AzureEventHubsService(azureEventProcessorFactory);
		}

		[Test]
		public async Task SendMessagesToEventHubAsyncTest()
		{
			for (int i = 0; i < 10; i++)
			{
				await _sut.SendEventToHubAsync(_eventHubPath, "message");
			}
		}

		[Test]
		public async Task ConsumeMessagesFromEventHubAsyncTest()
		{
			await _sut.ConsumeEventsFromHubAsync(_eventHubPath);
			await Task.Delay(10000);
			await _sut.StopConsumeEventsFromHub();
		}

		[Test]
		public async Task SendAndConsumeMessagesFromEventHubAsyncTest()
		{
			await _sut.ConsumeEventsFromHubAsync(_eventHubPath);

			for (int i = 0; i < 10; i++)
			{
				await _sut.SendEventToHubAsync(_eventHubPath, $"message{i}");
				await Task.Delay(1000);
			}

			await _sut.StopConsumeEventsFromHub();
			Assert.That(_genericEventsService.GetEvents().Count > 0);
		}
	}
}
