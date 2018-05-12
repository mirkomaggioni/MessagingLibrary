using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Messaging.Core.Models;
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

		public void Publish(string message, string exchange, string routingKey = "", string type = "fanout", bool durable = false)
		{
			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				if (!string.IsNullOrEmpty(exchange))
					channel.ExchangeDeclare(exchange, type, durable, false, null);

				var body = Encoding.UTF8.GetBytes(message);
				channel.BasicPublish(exchange, routingKey, true, null, body);
			}
		}

		public IEnumerable<GenericMessage> Get(string exchange, string queue, string routingKey = "", string type = "fanout", bool durable = false)
		{
			var messages = new List<GenericMessage>();

			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange, type, durable, false);
				channel.QueueDeclare(queue, durable, false, false, null);
				channel.QueueBind(queue, exchange, routingKey);

				var consumer = new EventingBasicConsumer(channel);
				var result = channel.BasicGet(queue, true);

				while (result != null)
				{
					messages.Add(new GenericMessage()
						{
							Body = Encoding.UTF8.GetString(result.Body),
							MessageId = result.BasicProperties.MessageId
						});

					result = channel.BasicGet(queue, true);
				}
			}

			return messages;
		}

		public async Task SubscribeAsync(string exchange, string queue, Action<GenericMessage> callback, CancellationTokenSource cancellationTokenSource, string routingKey = "", string type = "fanout", bool durable = false)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			var messages = new List<GenericMessage>();

			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange, type, durable, false);
				channel.QueueDeclare(queue, durable, false, false, null);
				channel.QueueBind(queue, exchange, routingKey);

				var consumer = new EventingBasicConsumer(channel);
				consumer.Received += (model, result) =>
				{
					var body = result.Body;
					var message = new GenericMessage()
					{
						Body = Encoding.UTF8.GetString(body),
						MessageId = result.BasicProperties.MessageId
					};

					callback(message);
				};

				channel.BasicConsume(queue, true, consumer);

				await Task.Run(async () =>
				{
					while (true)
						await Task.Delay(30000).ConfigureAwait(false);
				}, cancellationTokenSource.Token).ConfigureAwait(false);

				channel.BasicCancel(consumer.ConsumerTag);
			}
		}
	}
}
