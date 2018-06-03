using System.Collections.Generic;
using Messaging.Core.Models;

namespace Messaging.Core.Interfaces
{
	public interface IRabbitPublisher : IRabbitPublisherSetup
	{
		void Publish(IEnumerable<Payload> payloads);
	}
}
