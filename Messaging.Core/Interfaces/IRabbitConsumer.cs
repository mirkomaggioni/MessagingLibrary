using System;
using Messaging.Core.Models;

namespace Messaging.Core.Interfaces
{
	public interface IRabbitConsumer : IDisposable
	{
		void Setup(RabbitConfiguration rabbitConfiguration);
		void Get(IRabbitMessageHandler messageHandler);
		void Consume(IRabbitMessageHandler messageHandler);
	}
}
