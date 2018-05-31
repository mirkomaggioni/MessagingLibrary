using Messaging.Core.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace Messaging.Core.Interfaces
{
	public interface IRabbitMessageHandler
	{
		List<Payload> Payloads { get; set; }
		void Handle(BasicGetResult result);
		void Handle(object model, BasicDeliverEventArgs result);
	}
}