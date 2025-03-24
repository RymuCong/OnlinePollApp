
namespace FDS.CRM.CrossCuttingConcerns.Locks;

public interface IDistributedLockScope : IDisposable
{
    bool StillHoldingLock();
}
