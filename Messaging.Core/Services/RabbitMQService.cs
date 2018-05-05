using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Messaging.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Core.Services
{
	public class RabbitMQService
	{
		private readonly string _hostName = ConfigurationManager.ConnectionStrings["RabbitMQHostname"].ConnectionString;
		private readonly ConnectionFactory _factory;

		public RabbitMQService()
		{
			_factory = new ConnectionFactory() { HostName = _hostName };
		}

		public void SendMessage(string message, string exchange, string routingKey = "", string type = "fanout")
		{
			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				if (!string.IsNullOrEmpty(exchange))
					channel.ExchangeDeclare(exchange, type, false, false, null);

				var properties = channel.CreateBasicProperties();
				properties.Persistent = true;

				var body = Encoding.UTF8.GetBytes(message);
				channel.BasicPublish(exchange, routingKey, true, properties, body);
			}
		}

		public GenericMessage ReadMessageAsync(string exchange, string queue, string routingKey = "", string type = "fanout")
		{
			GenericMessage message = null;

			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange, type, false, false);
				channel.QueueDeclare(queue, false, false, false, null);
				channel.QueueBind(queue, exchange, routingKey);

				var consumer = new EventingBasicConsumer(channel);
				var result = channel.BasicGet(queue, true);

				if (result != null)
				{
					message = new GenericMessage()
					{
						Body = Encoding.UTF8.GetString(result.Body),
						MessageId = result.BasicProperties.MessageId
					};
				}
			}

			return message;
		}
	}
}
