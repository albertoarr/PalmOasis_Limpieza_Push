using System.Threading;
using System.Threading.Tasks;

namespace PalmOasis_Limpieza_Push.Models.Interfaces
{
	public interface IPushService
	{
		Task SendToTopicAsync(string topic, string text, DateTime tsUtc, CancellationToken ct = default);
	}

}
