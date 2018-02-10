using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Messaging.Services;

namespace ServiceBusTests.Services
{
	public class MSMQServiceTests
	{
		private readonly string _msmqQueue = ConfigurationManager.ConnectionStrings["MSMQQueue"].ConnectionString;
		private MSMQService _sut;

		[SetUp]
		public void Setup()
		{
			_sut = new MSMQService();
		}

		[Test]
		public void SendMessageToMSMQTest()
		{
			var result = false;

			for (int i = 0; i < 10; i++)
			{
				result = _sut.SendMessageToQueue(_msmqQueue, "message");
			}

			Assert.IsTrue(result);
		}

		[Test]
		public void ReadAllMessagesFromMSMQTest()
		{
			var results = _sut.ReadAllMessageFromQueue(_msmqQueue);
			Assert.That(results.Count() > 0);
			Assert.AreEqual(results.ElementAt(0).Body, "message");
		}

		[Test]
		public async Task ReadMessagesFromMSMQAsyncTest()
		{
			var results = await _sut.ReadMessageFromQueueAsync(_msmqQueue);
			Assert.That(results.Count() > 0);
			Assert.AreEqual(results.ElementAt(0).Body, "message");
		}
	}
}
