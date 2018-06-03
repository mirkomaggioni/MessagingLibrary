using Messaging.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Messaging.Core.Models
{
	public class DefaultRabbitConsumer : IRabbitConsumer
	{
		private RabbitConfiguration _rabbitConfiguration;
		private IConnection _connection;
		private IModel _channel;
		private EventingBasicConsumer _consumer;

		public IRabbitConsumer Setup(RabbitConfiguration rabbitConfiguration)
		{
			_rabbitConfiguration = rabbitConfiguration;
			_connection = _rabbitConfiguration.ConnectionFactory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.ExchangeDeclare(_rabbitConfiguration.Exchange, _rabbitConfiguration.Type, _rabbitConfiguration.Durable, false);
			_channel.QueueDeclare(_rabbitConfiguration.Queue, _rabbitConfiguration.Durable, false, false, null);
			_channel.QueueBind(_rabbitConfiguration.Queue, _rabbitConfiguration.Exchange, _rabbitConfiguration.RoutingKey);
			return this;
		}

		public void Get(IRabbitMessageHandler messageHandler)
		{
			if (_rabbitConfiguration == null)
				throw new ApplicationException("Rabbit configuration is missing.");

			var result = _channel.BasicGet(_rabbitConfiguration.Queue, true);
			while (result != null)
			{
				messageHandler.Handle(result);
				result = _channel.BasicGet(_rabbitConfiguration.Queue, true);
			}
		}

		public void Consume(IRabbitMessageHandler messageHandler)
		{
			if (_rabbitConfiguration == null)
				throw new ApplicationException("Rabbit configuration is missing.");

			if (messageHandler == null)
				throw new ArgumentNullException(nameof(messageHandler));

			_consumer = new EventingBasicConsumer(_channel);
			_consumer.Received += (model, result) =>
			{
				messageHandler.Handle(model, result);
				_channel.BasicAck(result.DeliveryTag, false);
			};

			_channel.BasicConsume(_rabbitConfiguration.Queue, false, _consumer);
		}

		public void Dispose()
		{
			if (_channel?.IsOpen == true && _consumer != null)
			{
				_channel.BasicCancel(_consumer.ConsumerTag);
			}

			_channel?.Dispose();
			_connection?.Dispose();
		}
	}
}
