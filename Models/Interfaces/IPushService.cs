using System.Threading;
using System.Threading.Tasks;

namespace PalmOasis_Limpieza_Push.Models.Interfaces
{
	public interface IPushService
	{
		Task SendRoomCleanedToTopicAsync(string topic, object dto, CancellationToken ct = default);
	}
}
