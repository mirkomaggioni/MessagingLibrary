using Messaging.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Core.Interfaces
{
	public interface IMessageConsumer
	{
		void Setup(RabbitConfiguration rabbitConfiguration);
		Task ConsumeAsync(IMessageHandler messageHandler, CancellationTokenSource cancellationTokenSource);
	}
}
