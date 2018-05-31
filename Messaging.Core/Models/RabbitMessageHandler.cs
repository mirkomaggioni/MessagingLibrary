using System.Collections.Generic;
using System.Text;
using Messaging.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Core.Models.Rabbit
{
	public class RabbitMessageHandler : IRabbitMessageHandler
	{
		public List<Payload> Payloads { get; set; } = new List<Payload>();

		public void Handle(BasicGetResult result)
		{
			Payloads.Add(new Payload()
			{
				Body = Encoding.UTF8.GetString(result.Body),
				MessageId = result.BasicProperties.MessageId,
				CorrelationId = result.BasicProperties.CorrelationId,
				ReplyTo = result.BasicProperties.ReplyTo
			});
		}

		public void Handle(object model, BasicDeliverEventArgs result)
		{
			Payloads.Add(new Payload()
			{
				Body = Encoding.UTF8.GetString(result.Body),
				MessageId = result.BasicProperties.MessageId,
				CorrelationId = result.BasicProperties.CorrelationId,
				ReplyTo = result.BasicProperties.ReplyTo
			});
		}
	}
}
