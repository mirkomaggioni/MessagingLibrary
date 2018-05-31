using Messaging.Core.Models;

namespace Messaging.Core.Interfaces
{
	public interface IRabbitPublisher
	{
		void Setup(RabbitConfiguration rabbitConfiguration);
		void Publish(Payload payload);
	}
}
