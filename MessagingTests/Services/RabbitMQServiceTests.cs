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
		public void ReadMessage()
		{
			var message = _sut.ReadMessageAsync("messages", "defaultqueue");
			Assert.IsNotNull(message);
			Assert.AreEqual(message.Body, "message");
		}
	}
}
