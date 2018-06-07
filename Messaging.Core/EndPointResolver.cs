using System.Collections.Generic;
using System.Configuration;
using RabbitMQ.Client;

namespace Messaging.Core
{
	public class EndPointResolver : IEndpointResolver
	{
		public IEnumerable<AmqpTcpEndpoint> All()
		{
			return new List<AmqpTcpEndpoint>()
			{
				new AmqpTcpEndpoint()
				{
					HostName = ConfigurationManager.ConnectionStrings["RabbitMQHostname1"].ConnectionString
				},
				new AmqpTcpEndpoint()
				{
					HostName = ConfigurationManager.ConnectionStrings["RabbitMQHostname2"].ConnectionString
				}
			};
		}
	}
}
