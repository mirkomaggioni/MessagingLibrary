using Messaging.Core.Interfaces;

namespace Messaging.Core.Models
{
	public class GenericMessage
	{
		public string Label { get; set; }
		public string MessageId { get; set; }
		public string CorrelationId { get; set; }
		public string ReplyTo { get; set; }
		public string Body { get; set; }
	}
}
