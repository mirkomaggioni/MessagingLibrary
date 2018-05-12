using System.Collections.Generic;
using Messaging.Core.Models;

namespace Messaging.Core.Services
{
	public class GenericEventsService
	{
		private List<GenericEvent> _events = new List<GenericEvent>();

		public void AddEvent(string data)
		{
			_events.Add(new GenericEvent() { Body = data });
		}

		public List<GenericEvent> GetEvents()
		{
			return _events;
		}
	}
}
