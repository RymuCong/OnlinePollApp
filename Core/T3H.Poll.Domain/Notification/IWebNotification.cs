using System.Threading;
using System.Threading.Tasks;

namespace T3H.Poll.Domain.Notification;

public interface IWebNotification<T>
{
    Task SendAsync(T message, CancellationToken cancellationToken = default);
}
