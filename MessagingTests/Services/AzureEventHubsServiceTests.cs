using System;
using System.Collections.Generic;
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

		[SetUp]
		public void Setup()
		{
			_sut = new AzureEventHubsService();
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

			await Task.Delay(20000);
			await _sut.StopConsumeEventsFromHub();
		}
	}
}
