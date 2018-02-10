using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Messaging.Services;

namespace ServiceBusTests.Services
{
	[TestFixture]
	public class AzureServiceBusServiceTests
	{
		private readonly string _serviceBusConnectionString = ConfigurationManager.ConnectionStrings["AzureServiceBus"].ConnectionString;
		private readonly string _serviceBusQueue = "queuemmtest1";
		private AzureServiceBusService _sut;

		[SetUp]
		public void Setup()
		{
			_sut = new AzureServiceBusService(_serviceBusConnectionString);
		}

		[Test]
		public async Task SendMessageToServiceBusQueueAsyncTest()
		{
			var result = false;

			for (int i = 0; i < 10; i++)
			{
				result = await _sut.SendMessageToQueueAsync(_serviceBusQueue, "message");
			}

			Assert.IsTrue(result);
		}

		[Test]
		public async Task ReadAllMessagesFromServiceBusQueueAsyncTest()
		{
			var results = await _sut.ReadAllMessageFromQueueAsync(_serviceBusQueue);
			Assert.That(results.Count() > 0);
			Assert.AreEqual(results.ElementAt(0).Body, "message");
		}

		[Test]
		public async Task SubscribeToServiceBusQueueAsyncTest()
		{
			var results = await _sut.ReadMessageFromQueueAsync(_serviceBusQueue);
			Assert.That(results.Count() > 0);
			Assert.AreEqual(results.ElementAt(0).Body, "message");
		}
	}
}
