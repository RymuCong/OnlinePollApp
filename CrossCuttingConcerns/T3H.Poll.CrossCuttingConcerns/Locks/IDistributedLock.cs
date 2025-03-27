namespace T3H.Poll.CrossCuttingConcerns.Locks;

public interface IDistributedLock : IDisposable
{
    IDistributedLockScope Acquire(string lockName);

    IDistributedLockScope TryAcquire(string lockName);
}
