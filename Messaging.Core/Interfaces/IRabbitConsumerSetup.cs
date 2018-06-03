using System;
using Messaging.Core.Models;

namespace Messaging.Core.Interfaces
{
	public interface IRabbitConsumerSetup : IDisposable
	{
		IRabbitConsumer Setup(RabbitConfiguration rabbitConfiguration);
	}
}
