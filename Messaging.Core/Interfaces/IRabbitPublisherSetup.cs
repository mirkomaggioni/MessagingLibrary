using Messaging.Core.Models;

namespace Messaging.Core.Interfaces
{
	public interface IRabbitPublisherSetup
	{
		IRabbitPublisher Setup(RabbitConfiguration rabbitConfiguration);
	}
}
