using System;
using System.Configuration;
using Autofac;
using Messaging.Core.Models;
using Messaging.Core.Services;
using RabbitMQ.Client;

namespace Messaging.Core
{
	public class MessagingCoreModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var connectionFactory = new ConnectionFactory
			{
				UserName = ConfigurationManager.ConnectionStrings["RabbitMQUsername"].ConnectionString,
				Password = ConfigurationManager.ConnectionStrings["RabbitMQPassword"].ConnectionString,
				RequestedHeartbeat = 10,
				AutomaticRecoveryEnabled = true,
				NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
			};

			connectionFactory.EndpointResolverFactory = (amqpTcpEndpoints) => new EndPointResolver();

			builder.Register(c => connectionFactory).SingleInstance();
			builder.RegisterType<RabbitService<DefaultRabbitPublisher, DefaultRabbitConsumer>>().SingleInstance();
		}
	}
}
