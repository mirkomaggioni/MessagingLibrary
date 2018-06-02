using Messaging.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Core.Models
{
	public class DefaultRabbitConsumer : IRabbitConsumer
	{
		private RabbitConfiguration _rabbitConfiguration;

		public void Setup(RabbitConfiguration rabbitConfiguration)
		{
			_rabbitConfiguration = rabbitConfiguration;
		}

		public void Get(IRabbitMessageHandler messageHandler)
		{
			if (_rabbitConfiguration == null)
				throw new ApplicationException("Rabbit configuration is missing.");

			using (var connection = _rabbitConfiguration.ConnectionFactory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_rabbitConfiguration.Exchange, _rabbitConfiguration.Type, _rabbitConfiguration.Durable, false);
				channel.QueueDeclare(_rabbitConfiguration.Queue, _rabbitConfiguration.Durable, false, false, null);
				channel.QueueBind(_rabbitConfiguration.Queue, _rabbitConfiguration.Exchange, _rabbitConfiguration.RoutingKey);

				var consumer = new EventingBasicConsumer(channel);
				var result = channel.BasicGet(_rabbitConfiguration.Queue, true);

				while (result != null)
				{
					messageHandler.Handle(result);
					result = channel.BasicGet(_rabbitConfiguration.Queue, true);
				}
			}
		}

		public async Task ConsumeAsync(IRabbitMessageHandler messageHandler, CancellationTokenSource cancellationTokenSource)
		{
			if (_rabbitConfiguration == null)
				throw new ApplicationException("Rabbit configuration is missing.");

			if (messageHandler == null)
				throw new ArgumentNullException(nameof(messageHandler));

			using (var connection = _rabbitConfiguration.ConnectionFactory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_rabbitConfiguration.Exchange, _rabbitConfiguration.Type, _rabbitConfiguration.Durable, false);
				channel.QueueDeclare(_rabbitConfiguration.Queue, _rabbitConfiguration.Durable, false, false, null);
				channel.QueueBind(_rabbitConfiguration.Queue, _rabbitConfiguration.Exchange, _rabbitConfiguration.RoutingKey);

				var consumer = new EventingBasicConsumer(channel);
				consumer.Received += (model, result) =>
				{
					messageHandler.Handle(model, result);
					channel.BasicAck(result.DeliveryTag, false);
				};

				channel.BasicConsume(_rabbitConfiguration.Queue, false, consumer);

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
