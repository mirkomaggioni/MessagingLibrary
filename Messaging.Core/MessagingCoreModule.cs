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
			builder.Register(c => new ConnectionFactory
			{
				HostName = ConfigurationManager.ConnectionStrings["RabbitMQHostname"].ConnectionString,
				RequestedHeartbeat = 30,
				AutomaticRecoveryEnabled = true,
				NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
			}).SingleInstance();

			builder.RegisterType<RabbitService<DefaultRabbitPublisher, DefaultRabbitConsumer>>().SingleInstance();
		}
	}
}
