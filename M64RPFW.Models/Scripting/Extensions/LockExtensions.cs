namespace M64RPFW.Models.Scripting.Extensions;

public static class LockExtensions
{
    private class ReadLockImpl : IDisposable
    {
        ReaderWriterLockSlim _rwlock;
        
        public ReadLockImpl(ReaderWriterLockSlim rwlock)
        {
            _rwlock = rwlock;
            _rwlock.EnterReadLock();
        }

        public void Dispose()
        {
            _rwlock.ExitReadLock();
        }
    }
    
    private class WriteLockImpl : IDisposable
    {
        ReaderWriterLockSlim _rwlock;
        
        public WriteLockImpl(ReaderWriterLockSlim rwlock)
        {
            _rwlock = rwlock;
            _rwlock.EnterWriteLock();
        }

        public void Dispose()
        {
            _rwlock.ExitWriteLock();
        }
    }

    public static IDisposable ReadLock(this ReaderWriterLockSlim rwlock)
    {
        return new ReadLockImpl(rwlock);
    }

    public static IDisposable WriteLock(this ReaderWriterLockSlim rwlock)
    {
        return new WriteLockImpl(rwlock);
    }
}