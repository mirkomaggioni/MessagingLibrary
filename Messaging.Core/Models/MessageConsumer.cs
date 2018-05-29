using Messaging.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Core.Models
{
	public class MessageConsumer : IMessageConsumer
	{
		private RabbitConfiguration _rabbitConfiguration;

		public void Setup(RabbitConfiguration rabbitConfiguration)
		{
			_rabbitConfiguration = rabbitConfiguration;
		}

		public async Task ConsumeAsync(IMessageHandler messageHandler, CancellationTokenSource cancellationTokenSource)
		{
			if (messageHandler == null)
				throw new ArgumentNullException(nameof(messageHandler));

			var disconnected = false;
			var messages = new List<GenericMessage>();

			using (var connection = _rabbitConfiguration.ConnectionFactory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_rabbitConfiguration.Exchange, _rabbitConfiguration.Type, _rabbitConfiguration.Durable, false);
				channel.QueueDeclare(_rabbitConfiguration.Queue, _rabbitConfiguration.Durable, false, false, null);
				channel.QueueBind(_rabbitConfiguration.Queue, _rabbitConfiguration.Exchange, _rabbitConfiguration.RoutingKey);

				var consumer = new EventingBasicConsumer(channel);
				consumer.Received += (model, result) =>
				{
					var body = result.Body;
					var message = new GenericMessage()
					{
						Body = Encoding.UTF8.GetString(body),
						MessageId = result.BasicProperties.MessageId,
						CorrelationId = result.BasicProperties.CorrelationId,
						ReplyTo = result.BasicProperties.ReplyTo
					};

					messageHandler.Handle(message);
					channel.BasicAck(result.DeliveryTag, false);
				};

				channel.BasicConsume(_rabbitConfiguration.Queue, false, consumer);

				await Task.Run(async () =>
				{
					while (!disconnected)
						await Task.Delay(30000).ConfigureAwait(false);
				}, cancellationTokenSource.Token).ConfigureAwait(false);

				channel.BasicCancel(consumer.ConsumerTag);
			}
		}
	}
}
