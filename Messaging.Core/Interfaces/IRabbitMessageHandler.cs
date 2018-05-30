using Messaging.Core.Models;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace Messaging.Core.Interfaces
{
	public interface IRabbitMessageHandler
	{
		List<GenericMessage> Messages { get; set; }
		void Handle(object model, BasicDeliverEventArgs result);
	}
}