using Messaging.Core.Models;
using System.Collections.Generic;

namespace Messaging.Core.Interfaces
{
	public interface IMessageHandler
	{
		List<GenericMessage> Messages { get; set; }
		void Handle(GenericMessage genericMessage);
	}
}