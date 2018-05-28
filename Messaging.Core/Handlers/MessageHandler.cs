using System.Collections.Generic;
using Messaging.Core.Interfaces;

namespace Messaging.Core.Models.Rabbit
{
	public class MessageHandler : IMessageHandler
	{
		public List<GenericMessage> Messages { get; set; } = new List<GenericMessage>();
		public void Handle(GenericMessage genericMessage)
		{
			Messages.Add(genericMessage);
		}
	}
}
