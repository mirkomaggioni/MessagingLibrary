namespace Messaging.Core.Interfaces
{
	public interface IRabbitConsumer : IRabbitConsumerSetup
	{
		void Get(IRabbitMessageHandler messageHandler);
		void Consume(IRabbitMessageHandler messageHandler);
	}
}
