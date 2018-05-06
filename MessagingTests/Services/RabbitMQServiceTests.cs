using System.Linq;
using Messaging.Core.Services;
using NUnit.Framework;

namespace ServiceBusTests.Services
{
	public class RabbitMQServiceTests
	{
		private RabbitMQService _sut;

		[SetUp]
		public void Setup()
		{
			_sut = new RabbitMQService();
		}

		[Test]
		public void SendMessages()
		{
			for (int i = 0; i < 10; i++)
			{
				_sut.SendMessage("message", exchange: "messages");
			}

			Assert.IsTrue(true);
		}

		[Test]
		public void ReadMessages()
		{
			var messages = _sut.ReadMessages("messages", "defaultqueue");
			Assert.IsNotNull(messages);
			Assert.AreEqual(messages.ElementAt(0).Body, "message");
		}
	}
}
